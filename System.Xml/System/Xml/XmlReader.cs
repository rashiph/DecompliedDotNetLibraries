namespace System.Xml
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;

    [DebuggerDisplay("{debuggerDisplayProxy}")]
    public abstract class XmlReader : IDisposable
    {
        internal const int BiggerBufferSize = 0x2000;
        private static uint CanReadContentAsBitmap = 0x1e1bc;
        internal const int DefaultBufferSize = 0x1000;
        private static uint HasValueBitmap = 0x2659c;
        private static uint IsTextualNodeBitmap = 0x6018;
        internal const int MaxStreamLengthForDefaultBufferSize = 0x10000;

        protected XmlReader()
        {
        }

        private static string AddLineInfo(string message, IXmlLineInfo lineInfo)
        {
            if (lineInfo != null)
            {
                string[] args = new string[] { lineInfo.LineNumber.ToString(CultureInfo.InvariantCulture), lineInfo.LinePosition.ToString(CultureInfo.InvariantCulture) };
                message = message + " " + Res.GetString("Xml_ErrorPosition", args);
            }
            return message;
        }

        internal static int CalcBufferSize(Stream input)
        {
            int num = 0x1000;
            if (input.CanSeek)
            {
                long length = input.Length;
                if (length < num)
                {
                    return (int) length;
                }
                if (length > 0x10000L)
                {
                    num = 0x2000;
                }
            }
            return num;
        }

        internal bool CanReadContentAs()
        {
            return CanReadContentAs(this.NodeType);
        }

        internal static bool CanReadContentAs(XmlNodeType nodeType)
        {
            return (0L != (CanReadContentAsBitmap & (((int) 1) << nodeType)));
        }

        internal void CheckElement(string localName, string namespaceURI)
        {
            if ((localName == null) || (localName.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            if (this.NodeType != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if ((this.LocalName != localName) || (this.NamespaceURI != namespaceURI))
            {
                throw new XmlException("Xml_ElementNotFoundNs", new string[] { localName, namespaceURI }, this as IXmlLineInfo);
            }
        }

        public virtual void Close()
        {
        }

        public static XmlReader Create(Stream input)
        {
            return Create(input, null, string.Empty);
        }

        public static XmlReader Create(TextReader input)
        {
            return Create(input, null, string.Empty);
        }

        public static XmlReader Create(string inputUri)
        {
            return Create(inputUri, null, null);
        }

        public static XmlReader Create(Stream input, XmlReaderSettings settings)
        {
            return Create(input, settings, string.Empty);
        }

        public static XmlReader Create(TextReader input, XmlReaderSettings settings)
        {
            return Create(input, settings, string.Empty);
        }

        public static XmlReader Create(string inputUri, XmlReaderSettings settings)
        {
            return Create(inputUri, settings, null);
        }

        public static XmlReader Create(XmlReader reader, XmlReaderSettings settings)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(reader);
        }

        public static XmlReader Create(Stream input, XmlReaderSettings settings, string baseUri)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(input, null, baseUri, null);
        }

        public static XmlReader Create(Stream input, XmlReaderSettings settings, XmlParserContext inputContext)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(input, null, string.Empty, inputContext);
        }

        public static XmlReader Create(TextReader input, XmlReaderSettings settings, string baseUri)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(input, baseUri, null);
        }

        public static XmlReader Create(TextReader input, XmlReaderSettings settings, XmlParserContext inputContext)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(input, string.Empty, inputContext);
        }

        public static XmlReader Create(string inputUri, XmlReaderSettings settings, XmlParserContext inputContext)
        {
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            return settings.CreateReader(inputUri, inputContext);
        }

        internal Exception CreateReadContentAsException(string methodName)
        {
            return CreateReadContentAsException(methodName, this.NodeType, this as IXmlLineInfo);
        }

        internal static Exception CreateReadContentAsException(string methodName, XmlNodeType nodeType, IXmlLineInfo lineInfo)
        {
            return new InvalidOperationException(AddLineInfo(Res.GetString("Xml_InvalidReadContentAs", new string[] { methodName, nodeType.ToString() }), lineInfo));
        }

        internal Exception CreateReadElementContentAsException(string methodName)
        {
            return CreateReadElementContentAsException(methodName, this.NodeType, this as IXmlLineInfo);
        }

        internal static Exception CreateReadElementContentAsException(string methodName, XmlNodeType nodeType, IXmlLineInfo lineInfo)
        {
            return new InvalidOperationException(AddLineInfo(Res.GetString("Xml_InvalidReadElementContentAs", new string[] { methodName, nodeType.ToString() }), lineInfo));
        }

        internal static XmlReader CreateSqlReader(Stream input, XmlReaderSettings settings, XmlParserContext inputContext)
        {
            XmlReader reader;
            int num2;
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            if (settings == null)
            {
                settings = new XmlReaderSettings();
            }
            byte[] buffer = new byte[CalcBufferSize(input)];
            int offset = 0;
            do
            {
                num2 = input.Read(buffer, offset, buffer.Length - offset);
                offset += num2;
            }
            while ((num2 > 0) && (offset < 2));
            if (((offset >= 2) && (buffer[0] == 0xdf)) && (buffer[1] == 0xff))
            {
                if (inputContext != null)
                {
                    throw new ArgumentException(Res.GetString("XmlBinary_NoParserContext"), "inputContext");
                }
                reader = new XmlSqlBinaryReader(input, buffer, offset, string.Empty, settings.CloseInput, settings);
            }
            else
            {
                reader = new XmlTextReaderImpl(input, buffer, offset, settings, null, string.Empty, inputContext, settings.CloseInput);
            }
            if (settings.ValidationType != ValidationType.None)
            {
                reader = settings.AddValidation(reader);
            }
            return reader;
        }

        private XmlWriter CreateWriterForInnerOuterXml(StringWriter sw)
        {
            XmlTextWriter xtw = new XmlTextWriter(sw);
            this.SetNamespacesFlag(xtw);
            return xtw;
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.ReadState != System.Xml.ReadState.Closed))
            {
                this.Close();
            }
        }

        private void FinishReadElementContentAsXxx()
        {
            if (this.NodeType != XmlNodeType.EndElement)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString());
            }
            this.Read();
        }

        public abstract string GetAttribute(int i);
        public abstract string GetAttribute(string name);
        public abstract string GetAttribute(string name, string namespaceURI);
        internal static Encoding GetEncoding(XmlReader reader)
        {
            XmlTextReaderImpl xmlTextReaderImpl = GetXmlTextReaderImpl(reader);
            if (xmlTextReaderImpl == null)
            {
                return null;
            }
            return xmlTextReaderImpl.Encoding;
        }

        internal static ConformanceLevel GetV1ConformanceLevel(XmlReader reader)
        {
            XmlTextReaderImpl xmlTextReaderImpl = GetXmlTextReaderImpl(reader);
            if (xmlTextReaderImpl == null)
            {
                return ConformanceLevel.Document;
            }
            return xmlTextReaderImpl.V1ComformanceLevel;
        }

        private static XmlTextReaderImpl GetXmlTextReaderImpl(XmlReader reader)
        {
            XmlTextReaderImpl impl = reader as XmlTextReaderImpl;
            if (impl != null)
            {
                return impl;
            }
            XmlTextReader reader2 = reader as XmlTextReader;
            if (reader2 != null)
            {
                return reader2.Impl;
            }
            XmlValidatingReaderImpl impl2 = reader as XmlValidatingReaderImpl;
            if (impl2 != null)
            {
                return impl2.ReaderImpl;
            }
            XmlValidatingReader reader3 = reader as XmlValidatingReader;
            if (reader3 != null)
            {
                return reader3.Impl.ReaderImpl;
            }
            return null;
        }

        internal static bool HasValueInternal(XmlNodeType nodeType)
        {
            return (0L != (HasValueBitmap & (((int) 1) << nodeType)));
        }

        internal string InternalReadContentAsString()
        {
            string str = string.Empty;
            StringBuilder builder = null;
            do
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Attribute:
                        return this.Value;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        if (str.Length != 0)
                        {
                            if (builder == null)
                            {
                                builder = new StringBuilder();
                                builder.Append(str);
                            }
                            builder.Append(this.Value);
                            break;
                        }
                        str = this.Value;
                        break;

                    case XmlNodeType.EntityReference:
                        if (!this.CanResolveEntity)
                        {
                            goto Label_00B6;
                        }
                        this.ResolveEntity();
                        break;

                    case XmlNodeType.Entity:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                    case XmlNodeType.EndElement:
                        goto Label_00B6;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                    case XmlNodeType.EndEntity:
                        break;

                    default:
                        goto Label_00B6;
                }
            }
            while ((this.AttributeCount != 0) ? this.ReadAttributeValue() : this.Read());
        Label_00B6:
            if (builder != null)
            {
                return builder.ToString();
            }
            return str;
        }

        public static bool IsName(string str)
        {
            if (str == null)
            {
                throw new NullReferenceException();
            }
            return ValidateNames.IsNameNoNamespaces(str);
        }

        public static bool IsNameToken(string str)
        {
            if (str == null)
            {
                throw new NullReferenceException();
            }
            return ValidateNames.IsNmtokenNoNamespaces(str);
        }

        public virtual bool IsStartElement()
        {
            return (this.MoveToContent() == XmlNodeType.Element);
        }

        public virtual bool IsStartElement(string name)
        {
            return ((this.MoveToContent() == XmlNodeType.Element) && (this.Name == name));
        }

        public virtual bool IsStartElement(string localname, string ns)
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                return false;
            }
            return ((this.LocalName == localname) && (this.NamespaceURI == ns));
        }

        internal static bool IsTextualNode(XmlNodeType nodeType)
        {
            return (0L != (IsTextualNodeBitmap & (((int) 1) << nodeType)));
        }

        public abstract string LookupNamespace(string prefix);
        public virtual void MoveToAttribute(int i)
        {
            if ((i < 0) || (i >= this.AttributeCount))
            {
                throw new ArgumentOutOfRangeException("i");
            }
            this.MoveToElement();
            this.MoveToFirstAttribute();
            for (int j = 0; j < i; j++)
            {
                this.MoveToNextAttribute();
            }
        }

        public abstract bool MoveToAttribute(string name);
        public abstract bool MoveToAttribute(string name, string ns);
        public virtual XmlNodeType MoveToContent()
        {
        Label_0000:
            switch (this.NodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                    break;

                case XmlNodeType.Attribute:
                    this.MoveToElement();
                    break;

                default:
                    if (this.Read())
                    {
                        goto Label_0000;
                    }
                    return this.NodeType;
            }
            return this.NodeType;
        }

        public abstract bool MoveToElement();
        public abstract bool MoveToFirstAttribute();
        public abstract bool MoveToNextAttribute();
        public abstract bool Read();
        public abstract bool ReadAttributeValue();
        public virtual object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            object obj2;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAs");
            }
            string str = this.InternalReadContentAsString();
            if (returnType == typeof(string))
            {
                return str;
            }
            try
            {
                obj2 = XmlUntypedConverter.Untyped.ChangeType(str, returnType, (namespaceResolver == null) ? (this as IXmlNamespaceResolver) : namespaceResolver);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception, this as IXmlLineInfo);
            }
            catch (InvalidCastException exception2)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", returnType.ToString(), exception2, this as IXmlLineInfo);
            }
            return obj2;
        }

        public virtual int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString("Xml_ReadBinaryContentNotSupported", new object[] { "ReadContentAsBase64" }));
        }

        public virtual int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString("Xml_ReadBinaryContentNotSupported", new object[] { "ReadContentAsBinHex" }));
        }

        public virtual bool ReadContentAsBoolean()
        {
            bool flag;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsBoolean");
            }
            try
            {
                flag = XmlConvert.ToBoolean(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Boolean", exception, this as IXmlLineInfo);
            }
            return flag;
        }

        public virtual DateTime ReadContentAsDateTime()
        {
            DateTime time;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsDateTime");
            }
            try
            {
                time = XmlConvert.ToDateTime(this.InternalReadContentAsString(), XmlDateTimeSerializationMode.RoundtripKind);
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "DateTime", exception, this as IXmlLineInfo);
            }
            return time;
        }

        public virtual decimal ReadContentAsDecimal()
        {
            decimal num;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsDecimal");
            }
            try
            {
                num = XmlConvert.ToDecimal(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Decimal", exception, this as IXmlLineInfo);
            }
            return num;
        }

        public virtual double ReadContentAsDouble()
        {
            double num;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsDouble");
            }
            try
            {
                num = XmlConvert.ToDouble(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Double", exception, this as IXmlLineInfo);
            }
            return num;
        }

        public virtual float ReadContentAsFloat()
        {
            float num;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsFloat");
            }
            try
            {
                num = XmlConvert.ToSingle(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Float", exception, this as IXmlLineInfo);
            }
            return num;
        }

        public virtual int ReadContentAsInt()
        {
            int num;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsInt");
            }
            try
            {
                num = XmlConvert.ToInt32(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Int", exception, this as IXmlLineInfo);
            }
            return num;
        }

        public virtual long ReadContentAsLong()
        {
            long num;
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsLong");
            }
            try
            {
                num = XmlConvert.ToInt64(this.InternalReadContentAsString());
            }
            catch (FormatException exception)
            {
                throw new XmlException("Xml_ReadContentAsFormatException", "Long", exception, this as IXmlLineInfo);
            }
            return num;
        }

        public virtual object ReadContentAsObject()
        {
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsObject");
            }
            return this.InternalReadContentAsString();
        }

        public virtual string ReadContentAsString()
        {
            if (!this.CanReadContentAs())
            {
                throw this.CreateReadContentAsException("ReadContentAsString");
            }
            return this.InternalReadContentAsString();
        }

        public virtual object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAs"))
            {
                object obj2 = this.ReadContentAs(returnType, namespaceResolver);
                this.FinishReadElementContentAsXxx();
                return obj2;
            }
            if (!(returnType == typeof(string)))
            {
                return XmlUntypedConverter.Untyped.ChangeType(string.Empty, returnType, namespaceResolver);
            }
            return string.Empty;
        }

        public virtual object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAs(returnType, namespaceResolver);
        }

        public virtual int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString("Xml_ReadBinaryContentNotSupported", new object[] { "ReadElementContentAsBase64" }));
        }

        public virtual int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString("Xml_ReadBinaryContentNotSupported", new object[] { "ReadElementContentAsBinHex" }));
        }

        public virtual bool ReadElementContentAsBoolean()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsBoolean"))
            {
                bool flag = this.ReadContentAsBoolean();
                this.FinishReadElementContentAsXxx();
                return flag;
            }
            return XmlConvert.ToBoolean(string.Empty);
        }

        public virtual bool ReadElementContentAsBoolean(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsBoolean();
        }

        public virtual DateTime ReadElementContentAsDateTime()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsDateTime"))
            {
                DateTime time = this.ReadContentAsDateTime();
                this.FinishReadElementContentAsXxx();
                return time;
            }
            return XmlConvert.ToDateTime(string.Empty, XmlDateTimeSerializationMode.RoundtripKind);
        }

        public virtual DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsDateTime();
        }

        public virtual decimal ReadElementContentAsDecimal()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsDecimal"))
            {
                decimal num = this.ReadContentAsDecimal();
                this.FinishReadElementContentAsXxx();
                return num;
            }
            return XmlConvert.ToDecimal(string.Empty);
        }

        public virtual decimal ReadElementContentAsDecimal(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsDecimal();
        }

        public virtual double ReadElementContentAsDouble()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsDouble"))
            {
                double num = this.ReadContentAsDouble();
                this.FinishReadElementContentAsXxx();
                return num;
            }
            return XmlConvert.ToDouble(string.Empty);
        }

        public virtual double ReadElementContentAsDouble(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsDouble();
        }

        public virtual float ReadElementContentAsFloat()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsFloat"))
            {
                float num = this.ReadContentAsFloat();
                this.FinishReadElementContentAsXxx();
                return num;
            }
            return XmlConvert.ToSingle(string.Empty);
        }

        public virtual float ReadElementContentAsFloat(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsFloat();
        }

        public virtual int ReadElementContentAsInt()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsInt"))
            {
                int num = this.ReadContentAsInt();
                this.FinishReadElementContentAsXxx();
                return num;
            }
            return XmlConvert.ToInt32(string.Empty);
        }

        public virtual int ReadElementContentAsInt(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsInt();
        }

        public virtual long ReadElementContentAsLong()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsLong"))
            {
                long num = this.ReadContentAsLong();
                this.FinishReadElementContentAsXxx();
                return num;
            }
            return XmlConvert.ToInt64(string.Empty);
        }

        public virtual long ReadElementContentAsLong(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsLong();
        }

        public virtual object ReadElementContentAsObject()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsObject"))
            {
                object obj2 = this.ReadContentAsObject();
                this.FinishReadElementContentAsXxx();
                return obj2;
            }
            return string.Empty;
        }

        public virtual object ReadElementContentAsObject(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsObject();
        }

        public virtual string ReadElementContentAsString()
        {
            if (this.SetupReadElementContentAsXxx("ReadElementContentAsString"))
            {
                string str = this.ReadContentAsString();
                this.FinishReadElementContentAsXxx();
                return str;
            }
            return string.Empty;
        }

        public virtual string ReadElementContentAsString(string localName, string namespaceURI)
        {
            this.CheckElement(localName, namespaceURI);
            return this.ReadElementContentAsString();
        }

        public virtual string ReadElementString()
        {
            string str = string.Empty;
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if (!this.IsEmptyElement)
            {
                this.Read();
                str = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_UnexpectedNodeInSimpleContent", new string[] { this.NodeType.ToString(), "ReadElementString" }, this as IXmlLineInfo);
                }
                this.Read();
                return str;
            }
            this.Read();
            return str;
        }

        public virtual string ReadElementString(string name)
        {
            string str = string.Empty;
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if (this.Name != name)
            {
                throw new XmlException("Xml_ElementNotFound", name, this as IXmlLineInfo);
            }
            if (!this.IsEmptyElement)
            {
                str = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
                }
                this.Read();
                return str;
            }
            this.Read();
            return str;
        }

        public virtual string ReadElementString(string localname, string ns)
        {
            string str = string.Empty;
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if ((this.LocalName != localname) || (this.NamespaceURI != ns))
            {
                throw new XmlException("Xml_ElementNotFoundNs", new string[] { localname, ns }, this as IXmlLineInfo);
            }
            if (!this.IsEmptyElement)
            {
                str = this.ReadString();
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
                }
                this.Read();
                return str;
            }
            this.Read();
            return str;
        }

        public virtual void ReadEndElement()
        {
            if (this.MoveToContent() != XmlNodeType.EndElement)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            this.Read();
        }

        public virtual string ReadInnerXml()
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return string.Empty;
            }
            if ((this.NodeType != XmlNodeType.Attribute) && (this.NodeType != XmlNodeType.Element))
            {
                this.Read();
                return string.Empty;
            }
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriter xtw = this.CreateWriterForInnerOuterXml(sw);
            try
            {
                if (this.NodeType == XmlNodeType.Attribute)
                {
                    ((XmlTextWriter) xtw).QuoteChar = this.QuoteChar;
                    this.WriteAttributeValue(xtw);
                }
                if (this.NodeType == XmlNodeType.Element)
                {
                    this.WriteNode(xtw, false);
                }
            }
            finally
            {
                xtw.Close();
            }
            return sw.ToString();
        }

        public virtual string ReadOuterXml()
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return string.Empty;
            }
            if ((this.NodeType != XmlNodeType.Attribute) && (this.NodeType != XmlNodeType.Element))
            {
                this.Read();
                return string.Empty;
            }
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            XmlWriter xtw = this.CreateWriterForInnerOuterXml(sw);
            try
            {
                if (this.NodeType == XmlNodeType.Attribute)
                {
                    xtw.WriteStartAttribute(this.Prefix, this.LocalName, this.NamespaceURI);
                    this.WriteAttributeValue(xtw);
                    xtw.WriteEndAttribute();
                }
                else
                {
                    xtw.WriteNode(this, false);
                }
            }
            finally
            {
                xtw.Close();
            }
            return sw.ToString();
        }

        public virtual void ReadStartElement()
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            this.Read();
        }

        public virtual void ReadStartElement(string name)
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if (this.Name != name)
            {
                throw new XmlException("Xml_ElementNotFound", name, this as IXmlLineInfo);
            }
            this.Read();
        }

        public virtual void ReadStartElement(string localname, string ns)
        {
            if (this.MoveToContent() != XmlNodeType.Element)
            {
                throw new XmlException("Xml_InvalidNodeType", this.NodeType.ToString(), this as IXmlLineInfo);
            }
            if ((this.LocalName != localname) || (this.NamespaceURI != ns))
            {
                throw new XmlException("Xml_ElementNotFoundNs", new string[] { localname, ns }, this as IXmlLineInfo);
            }
            this.Read();
        }

        public virtual string ReadString()
        {
            if (this.ReadState != System.Xml.ReadState.Interactive)
            {
                return string.Empty;
            }
            this.MoveToElement();
            if (this.NodeType == XmlNodeType.Element)
            {
                if (this.IsEmptyElement)
                {
                    return string.Empty;
                }
                if (!this.Read())
                {
                    throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
                }
                if (this.NodeType == XmlNodeType.EndElement)
                {
                    return string.Empty;
                }
            }
            string str = string.Empty;
            while (IsTextualNode(this.NodeType))
            {
                str = str + this.Value;
                if (!this.Read())
                {
                    return str;
                }
            }
            return str;
        }

        public virtual XmlReader ReadSubtree()
        {
            if (this.NodeType != XmlNodeType.Element)
            {
                throw new InvalidOperationException(Res.GetString("Xml_ReadSubtreeNotOnElement"));
            }
            return new XmlSubtreeReader(this);
        }

        public virtual bool ReadToDescendant(string name)
        {
            if ((name == null) || (name.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
            }
            int depth = this.Depth;
            if (this.NodeType != XmlNodeType.Element)
            {
                if (this.ReadState != System.Xml.ReadState.Initial)
                {
                    return false;
                }
                depth--;
            }
            else if (this.IsEmptyElement)
            {
                return false;
            }
            name = this.NameTable.Add(name);
            while (this.Read() && (this.Depth > depth))
            {
                if ((this.NodeType == XmlNodeType.Element) && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ReadToDescendant(string localName, string namespaceURI)
        {
            if ((localName == null) || (localName.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            int depth = this.Depth;
            if (this.NodeType != XmlNodeType.Element)
            {
                if (this.ReadState != System.Xml.ReadState.Initial)
                {
                    return false;
                }
                depth--;
            }
            else if (this.IsEmptyElement)
            {
                return false;
            }
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);
            while (this.Read() && (this.Depth > depth))
            {
                if (((this.NodeType == XmlNodeType.Element) && Ref.Equal(localName, this.LocalName)) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ReadToFollowing(string name)
        {
            if ((name == null) || (name.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
            }
            name = this.NameTable.Add(name);
            while (this.Read())
            {
                if ((this.NodeType == XmlNodeType.Element) && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ReadToFollowing(string localName, string namespaceURI)
        {
            if ((localName == null) || (localName.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);
            while (this.Read())
            {
                if (((this.NodeType == XmlNodeType.Element) && Ref.Equal(localName, this.LocalName)) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool ReadToNextSibling(string name)
        {
            XmlNodeType nodeType;
            if ((name == null) || (name.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(name, "name");
            }
            name = this.NameTable.Add(name);
            do
            {
                if (!this.SkipSubtree())
                {
                    break;
                }
                nodeType = this.NodeType;
                if ((nodeType == XmlNodeType.Element) && Ref.Equal(name, this.Name))
                {
                    return true;
                }
            }
            while ((nodeType != XmlNodeType.EndElement) && !this.EOF);
            return false;
        }

        public virtual bool ReadToNextSibling(string localName, string namespaceURI)
        {
            XmlNodeType nodeType;
            if ((localName == null) || (localName.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(localName, "localName");
            }
            if (namespaceURI == null)
            {
                throw new ArgumentNullException("namespaceURI");
            }
            localName = this.NameTable.Add(localName);
            namespaceURI = this.NameTable.Add(namespaceURI);
            do
            {
                if (!this.SkipSubtree())
                {
                    break;
                }
                nodeType = this.NodeType;
                if (((nodeType == XmlNodeType.Element) && Ref.Equal(localName, this.LocalName)) && Ref.Equal(namespaceURI, this.NamespaceURI))
                {
                    return true;
                }
            }
            while ((nodeType != XmlNodeType.EndElement) && !this.EOF);
            return false;
        }

        public virtual int ReadValueChunk(char[] buffer, int index, int count)
        {
            throw new NotSupportedException(Res.GetString("Xml_ReadValueChunkNotSupported"));
        }

        public abstract void ResolveEntity();
        private void SetNamespacesFlag(XmlTextWriter xtw)
        {
            XmlTextReader reader = this as XmlTextReader;
            if (reader != null)
            {
                xtw.Namespaces = reader.Namespaces;
            }
            else
            {
                XmlValidatingReader reader2 = this as XmlValidatingReader;
                if (reader2 != null)
                {
                    xtw.Namespaces = reader2.Namespaces;
                }
            }
        }

        private bool SetupReadElementContentAsXxx(string methodName)
        {
            if (this.NodeType != XmlNodeType.Element)
            {
                throw this.CreateReadElementContentAsException(methodName);
            }
            bool isEmptyElement = this.IsEmptyElement;
            this.Read();
            if (isEmptyElement)
            {
                return false;
            }
            switch (this.NodeType)
            {
                case XmlNodeType.EndElement:
                    this.Read();
                    return false;

                case XmlNodeType.Element:
                    throw new XmlException("Xml_MixedReadElementContentAs", string.Empty, this as IXmlLineInfo);
            }
            return true;
        }

        public virtual void Skip()
        {
            if (this.ReadState == System.Xml.ReadState.Interactive)
            {
                this.SkipSubtree();
            }
        }

        private bool SkipSubtree()
        {
            this.MoveToElement();
            if ((this.NodeType == XmlNodeType.Element) && !this.IsEmptyElement)
            {
                int depth = this.Depth;
                while (this.Read() && (depth < this.Depth))
                {
                }
                if (this.NodeType != XmlNodeType.EndElement)
                {
                    return false;
                }
            }
            return this.Read();
        }

        private void WriteAttributeValue(XmlWriter xtw)
        {
            string name = this.Name;
            while (this.ReadAttributeValue())
            {
                if (this.NodeType == XmlNodeType.EntityReference)
                {
                    xtw.WriteEntityRef(this.Name);
                }
                else
                {
                    xtw.WriteString(this.Value);
                }
            }
            this.MoveToAttribute(name);
        }

        private void WriteNode(XmlWriter xtw, bool defattr)
        {
            int num = (this.NodeType == XmlNodeType.None) ? -1 : this.Depth;
            while (this.Read() && (num < this.Depth))
            {
                switch (this.NodeType)
                {
                    case XmlNodeType.Element:
                        xtw.WriteStartElement(this.Prefix, this.LocalName, this.NamespaceURI);
                        ((XmlTextWriter) xtw).QuoteChar = this.QuoteChar;
                        xtw.WriteAttributes(this, defattr);
                        if (this.IsEmptyElement)
                        {
                            xtw.WriteEndElement();
                        }
                        break;

                    case XmlNodeType.Text:
                        xtw.WriteString(this.Value);
                        break;

                    case XmlNodeType.CDATA:
                        xtw.WriteCData(this.Value);
                        break;

                    case XmlNodeType.EntityReference:
                        xtw.WriteEntityRef(this.Name);
                        break;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.XmlDeclaration:
                        xtw.WriteProcessingInstruction(this.Name, this.Value);
                        break;

                    case XmlNodeType.Comment:
                        xtw.WriteComment(this.Value);
                        break;

                    case XmlNodeType.DocumentType:
                        xtw.WriteDocType(this.Name, this.GetAttribute("PUBLIC"), this.GetAttribute("SYSTEM"), this.Value);
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        xtw.WriteWhitespace(this.Value);
                        break;

                    case XmlNodeType.EndElement:
                        xtw.WriteFullEndElement();
                        break;
                }
            }
            if ((num == this.Depth) && (this.NodeType == XmlNodeType.EndElement))
            {
                this.Read();
            }
        }

        public abstract int AttributeCount { get; }

        public abstract string BaseURI { get; }

        public virtual bool CanReadBinaryContent
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanReadValueChunk
        {
            get
            {
                return false;
            }
        }

        public virtual bool CanResolveEntity
        {
            get
            {
                return false;
            }
        }

        private object debuggerDisplayProxy
        {
            get
            {
                return new XmlReaderDebuggerDisplayProxy(this);
            }
        }

        public abstract int Depth { get; }

        internal virtual IDtdInfo DtdInfo
        {
            get
            {
                return null;
            }
        }

        public abstract bool EOF { get; }

        public virtual bool HasAttributes
        {
            get
            {
                return (this.AttributeCount > 0);
            }
        }

        public virtual bool HasValue
        {
            get
            {
                return HasValueInternal(this.NodeType);
            }
        }

        public virtual bool IsDefault
        {
            get
            {
                return false;
            }
        }

        internal bool IsDefaultInternal
        {
            get
            {
                if (this.IsDefault)
                {
                    return true;
                }
                IXmlSchemaInfo schemaInfo = this.SchemaInfo;
                return ((schemaInfo != null) && schemaInfo.IsDefault);
            }
        }

        public abstract bool IsEmptyElement { get; }

        public virtual string this[int i]
        {
            get
            {
                return this.GetAttribute(i);
            }
        }

        public virtual string this[string name]
        {
            get
            {
                return this.GetAttribute(name);
            }
        }

        public virtual string this[string name, string namespaceURI]
        {
            get
            {
                return this.GetAttribute(name, namespaceURI);
            }
        }

        public abstract string LocalName { get; }

        public virtual string Name
        {
            get
            {
                if (this.Prefix.Length == 0)
                {
                    return this.LocalName;
                }
                return this.NameTable.Add(this.Prefix + ":" + this.LocalName);
            }
        }

        internal virtual XmlNamespaceManager NamespaceManager
        {
            get
            {
                return null;
            }
        }

        public abstract string NamespaceURI { get; }

        public abstract XmlNameTable NameTable { get; }

        public abstract XmlNodeType NodeType { get; }

        public abstract string Prefix { get; }

        public virtual char QuoteChar
        {
            get
            {
                return '"';
            }
        }

        public abstract System.Xml.ReadState ReadState { get; }

        public virtual IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return (this as IXmlSchemaInfo);
            }
        }

        public virtual XmlReaderSettings Settings
        {
            get
            {
                return null;
            }
        }

        public abstract string Value { get; }

        public virtual Type ValueType
        {
            get
            {
                return typeof(string);
            }
        }

        public virtual string XmlLang
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return System.Xml.XmlSpace.None;
            }
        }

        [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{ToString()}")]
        private struct XmlReaderDebuggerDisplayProxy
        {
            private XmlReader reader;
            internal XmlReaderDebuggerDisplayProxy(XmlReader reader)
            {
                this.reader = reader;
            }

            public override string ToString()
            {
                XmlNodeType nodeType = this.reader.NodeType;
                string str = nodeType.ToString();
                switch (nodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.EndElement:
                    case XmlNodeType.EndEntity:
                    {
                        object obj2 = str;
                        return string.Concat(new object[] { obj2, ", Name=\"", this.reader.Name, '"' });
                    }
                    case XmlNodeType.Attribute:
                    case XmlNodeType.ProcessingInstruction:
                    {
                        object obj3 = str;
                        return string.Concat(new object[] { obj3, ", Name=\"", this.reader.Name, "\", Value=\"", XmlConvert.EscapeValueForDebuggerDisplay(this.reader.Value), '"' });
                    }
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Comment:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                    case XmlNodeType.XmlDeclaration:
                    {
                        object obj4 = str;
                        return string.Concat(new object[] { obj4, ", Value=\"", XmlConvert.EscapeValueForDebuggerDisplay(this.reader.Value), '"' });
                    }
                    case XmlNodeType.Entity:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                        return str;

                    case XmlNodeType.DocumentType:
                    {
                        object obj5 = str + ", Name=\"" + this.reader.Name + "'";
                        object obj6 = string.Concat(new object[] { obj5, ", SYSTEM=\"", this.reader.GetAttribute("SYSTEM"), '"' });
                        object obj7 = string.Concat(new object[] { obj6, ", PUBLIC=\"", this.reader.GetAttribute("PUBLIC"), '"' });
                        return string.Concat(new object[] { obj7, ", Value=\"", XmlConvert.EscapeValueForDebuggerDisplay(this.reader.Value), '"' });
                    }
                }
                return str;
            }
        }
    }
}

