namespace System.Data.SqlTypes
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, XmlSchemaProvider("GetXsdType")]
    public sealed class SqlXml : INullable, IXmlSerializable
    {
        private MethodInfo createSqlReaderMethodInfo;
        private bool firstCreateReader;
        private bool m_fNotNull;
        private Stream m_stream;
        private static readonly Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader> sqlReaderDelegate = CreateSqlReaderDelegate();

        public SqlXml()
        {
            this.SetNull();
        }

        private SqlXml(bool fNull)
        {
            this.SetNull();
        }

        public SqlXml(Stream value)
        {
            if (value == null)
            {
                this.SetNull();
            }
            else
            {
                this.firstCreateReader = true;
                this.m_fNotNull = true;
                this.m_stream = value;
            }
        }

        public SqlXml(XmlReader value)
        {
            if (value == null)
            {
                this.SetNull();
            }
            else
            {
                this.m_fNotNull = true;
                this.firstCreateReader = true;
                this.m_stream = this.CreateMemoryStreamFromXmlReader(value);
            }
        }

        private Stream CreateMemoryStreamFromXmlReader(XmlReader reader)
        {
            XmlWriterSettings settings = new XmlWriterSettings {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Fragment,
                Encoding = Encoding.GetEncoding("utf-16"),
                OmitXmlDeclaration = true
            };
            MemoryStream output = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(output, settings);
            if (reader.ReadState == ReadState.Closed)
            {
                throw new InvalidOperationException(SQLResource.ClosedXmlReaderMessage);
            }
            if (reader.ReadState == ReadState.Initial)
            {
                reader.Read();
            }
            while (!reader.EOF)
            {
                writer.WriteNode(reader, true);
            }
            writer.Flush();
            output.Seek(0L, SeekOrigin.Begin);
            return output;
        }

        public XmlReader CreateReader()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            SqlXmlStreamWrapper wrapper = new SqlXmlStreamWrapper(this.m_stream);
            if ((!this.firstCreateReader || wrapper.CanSeek) && (wrapper.Position != 0L))
            {
                wrapper.Seek(0L, SeekOrigin.Begin);
            }
            XmlReader reader = null;
            XmlReaderSettings settings = new XmlReaderSettings {
                ConformanceLevel = ConformanceLevel.Fragment
            };
            if (this.createSqlReaderMethodInfo == null)
            {
                this.createSqlReaderMethodInfo = typeof(XmlReader).GetMethod("CreateSqlReader", BindingFlags.NonPublic | BindingFlags.Static);
            }
            reader = sqlReaderDelegate(wrapper, settings, null);
            this.firstCreateReader = false;
            return reader;
        }

        [ReflectionPermission(SecurityAction.Assert, MemberAccess=true)]
        private static Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader> CreateSqlReaderDelegate()
        {
            MethodInfo method = typeof(XmlReader).GetMethod("CreateSqlReader", BindingFlags.NonPublic | BindingFlags.Static);
            return (Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader>) Delegate.CreateDelegate(typeof(Func<Stream, XmlReaderSettings, XmlParserContext, XmlReader>), method);
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
        }

        private void SetNull()
        {
            this.m_fNotNull = false;
            this.m_stream = null;
            this.firstCreateReader = true;
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader r)
        {
            string attribute = r.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                r.ReadInnerXml();
                this.SetNull();
            }
            else
            {
                this.m_fNotNull = true;
                this.firstCreateReader = true;
                this.m_stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(this.m_stream);
                writer.Write(r.ReadInnerXml());
                writer.Flush();
                if (this.m_stream.CanSeek)
                {
                    this.m_stream.Seek(0L, SeekOrigin.Begin);
                }
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                XmlReader reader = this.CreateReader();
                if (reader.ReadState == ReadState.Initial)
                {
                    reader.Read();
                }
                while (!reader.EOF)
                {
                    writer.WriteNode(reader, true);
                }
            }
            writer.Flush();
        }

        public bool IsNull
        {
            get
            {
                return !this.m_fNotNull;
            }
        }

        public static SqlXml Null
        {
            get
            {
                return new SqlXml(true);
            }
        }

        public string Value
        {
            get
            {
                if (this.IsNull)
                {
                    throw new SqlNullValueException();
                }
                StringWriter output = new StringWriter(null);
                XmlWriterSettings settings = new XmlWriterSettings {
                    CloseOutput = false,
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                XmlWriter writer = XmlWriter.Create(output, settings);
                XmlReader reader = this.CreateReader();
                if (reader.ReadState == ReadState.Initial)
                {
                    reader.Read();
                }
                while (!reader.EOF)
                {
                    writer.WriteNode(reader, true);
                }
                writer.Flush();
                return output.ToString();
            }
        }
    }
}

