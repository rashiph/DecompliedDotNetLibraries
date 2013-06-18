namespace System.Xaml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    public static class XamlServices
    {
        public static object Load(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            using (XmlReader reader = XmlReader.Create(stream))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader);
                return Load(xamlReader);
            }
        }

        public static object Load(TextReader textReader)
        {
            if (textReader == null)
            {
                throw new ArgumentNullException("textReader");
            }
            using (XmlReader reader = XmlReader.Create(textReader))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader);
                return Load(xamlReader);
            }
        }

        public static object Load(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            using (XmlReader reader = XmlReader.Create(fileName))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader);
                return Load(xamlReader);
            }
        }

        public static object Load(XamlReader xamlReader)
        {
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }
            XamlObjectWriter xamlWriter = new XamlObjectWriter(xamlReader.SchemaContext);
            Transform(xamlReader, xamlWriter);
            return xamlWriter.Result;
        }

        public static object Load(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            using (XamlXmlReader reader = new XamlXmlReader(xmlReader))
            {
                return Load(reader);
            }
        }

        public static object Parse(string xaml)
        {
            if (xaml == null)
            {
                throw new ArgumentNullException("xaml");
            }
            StringReader input = new StringReader(xaml);
            using (XmlReader reader2 = XmlReader.Create(input))
            {
                XamlXmlReader xamlReader = new XamlXmlReader(reader2);
                return Load(xamlReader);
            }
        }

        public static string Save(object instance)
        {
            StringWriter output = new StringWriter(CultureInfo.CurrentCulture);
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer2 = XmlWriter.Create(output, settings))
            {
                Save(writer2, instance);
            }
            return output.ToString();
        }

        public static void Save(Stream stream, object instance)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(stream, settings))
            {
                Save(writer, instance);
                writer.Flush();
            }
        }

        public static void Save(TextWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer2 = XmlWriter.Create(writer, settings))
            {
                Save(writer2, instance);
                writer2.Flush();
            }
        }

        public static void Save(string fileName, object instance)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(System.Xaml.SR.Get("StringIsNullOrEmpty"), "fileName");
            }
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                OmitXmlDeclaration = true
            };
            using (XmlWriter writer = XmlWriter.Create(fileName, settings))
            {
                Save(writer, instance);
                writer.Flush();
            }
        }

        public static void Save(XamlWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            XamlObjectReader xamlReader = new XamlObjectReader(instance, writer.SchemaContext);
            Transform(xamlReader, writer);
        }

        public static void Save(XmlWriter writer, object instance)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            using (XamlXmlWriter writer2 = new XamlXmlWriter(writer, new XamlSchemaContext()))
            {
                Save(writer2, instance);
            }
        }

        public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter)
        {
            Transform(xamlReader, xamlWriter, true);
        }

        public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
        {
            if (xamlReader == null)
            {
                throw new ArgumentNullException("xamlReader");
            }
            if (xamlWriter == null)
            {
                throw new ArgumentNullException("xamlWriter");
            }
            IXamlLineInfo info = xamlReader as IXamlLineInfo;
            IXamlLineInfoConsumer consumer = xamlWriter as IXamlLineInfoConsumer;
            bool flag = false;
            if (((info != null) && info.HasLineInfo) && ((consumer != null) && consumer.ShouldProvideLineInfo))
            {
                flag = true;
            }
            while (xamlReader.Read())
            {
                if (flag && (info.LineNumber != 0))
                {
                    consumer.SetLineInfo(info.LineNumber, info.LinePosition);
                }
                xamlWriter.WriteNode(xamlReader);
            }
            if (closeWriter)
            {
                xamlWriter.Close();
            }
        }
    }
}

