namespace System.Xml
{
    using System;
    using System.Xml.Schema;
    using System.Xml.XPath;

    internal abstract class XmlRawWriter : XmlWriter
    {
        protected XmlRawWriterBase64Encoder base64Encoder;
        protected IXmlNamespaceResolver resolver;

        protected XmlRawWriter()
        {
        }

        internal virtual void Close(System.Xml.WriteState currentState)
        {
            this.Close();
        }

        public override string LookupPrefix(string ns)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal virtual void OnRootElement(ConformanceLevel conformanceLevel)
        {
        }

        internal abstract void StartElementContent();
        public override void WriteAttributes(XmlReader reader, bool defattr)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            if (this.base64Encoder == null)
            {
                this.base64Encoder = new XmlRawWriterBase64Encoder(this);
            }
            this.base64Encoder.Encode(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.WriteString(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.WriteString(new string(new char[] { ch }));
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        internal virtual void WriteEndBase64()
        {
            this.base64Encoder.Flush();
        }

        public override void WriteEndDocument()
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteEndElement()
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal abstract void WriteEndElement(string prefix, string localName, string ns);
        internal virtual void WriteEndNamespaceDeclaration()
        {
            throw new NotSupportedException();
        }

        public override void WriteFullEndElement()
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal virtual void WriteFullEndElement(string prefix, string localName, string ns)
        {
            this.WriteEndElement(prefix, localName, ns);
        }

        public override void WriteName(string name)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal abstract void WriteNamespaceDeclaration(string prefix, string ns);
        public override void WriteNmToken(string name)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteNode(XmlReader reader, bool defattr)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteNode(XPathNavigator navigator, bool defattr)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal virtual void WriteQualifiedName(string prefix, string localName, string ns)
        {
            if (prefix.Length != 0)
            {
                this.WriteString(prefix);
                this.WriteString(":");
            }
            this.WriteString(localName);
        }

        public override void WriteRaw(string data)
        {
            this.WriteString(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteString(new string(buffer, index, count));
        }

        public override void WriteStartDocument()
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        public override void WriteStartDocument(bool standalone)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        internal virtual void WriteStartNamespaceDeclaration(string prefix)
        {
            throw new NotSupportedException();
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.WriteString(new string(new char[] { lowChar, highChar }));
        }

        public override void WriteValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.WriteString(XmlUntypedConverter.Untyped.ToString(value, this.resolver));
        }

        public override void WriteValue(string value)
        {
            this.WriteString(value);
        }

        public override void WriteWhitespace(string ws)
        {
            this.WriteString(ws);
        }

        internal virtual void WriteXmlDeclaration(string xmldecl)
        {
        }

        internal virtual void WriteXmlDeclaration(XmlStandalone standalone)
        {
        }

        internal virtual IXmlNamespaceResolver NamespaceResolver
        {
            get
            {
                return this.resolver;
            }
            set
            {
                this.resolver = value;
            }
        }

        internal virtual bool SupportsNamespaceDeclarationInChunks
        {
            get
            {
                return false;
            }
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
            }
        }
    }
}

