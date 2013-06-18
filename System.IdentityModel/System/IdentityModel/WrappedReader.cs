namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Xml;

    internal sealed class WrappedReader : XmlDictionaryReader, IXmlLineInfo
    {
        private TextReader contentReader;
        private MemoryStream contentStream;
        private int depth;
        private readonly XmlDictionaryReader reader;
        private bool recordDone;
        private readonly XmlTokenStream xmlTokens;

        public WrappedReader(XmlDictionaryReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("reader"));
            }
            if (!reader.IsStartElement())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("InnerReaderMustBeAtElement")));
            }
            this.xmlTokens = new XmlTokenStream(0x20);
            this.reader = reader;
            this.Record();
        }

        public override void Close()
        {
            this.OnEndOfContent();
            this.reader.Close();
        }

        public override string GetAttribute(int index)
        {
            return this.reader.GetAttribute(index);
        }

        public override string GetAttribute(string name)
        {
            return this.reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string ns)
        {
            return this.reader.GetAttribute(name, ns);
        }

        public bool HasLineInfo()
        {
            IXmlLineInfo reader = this.reader as IXmlLineInfo;
            return ((reader != null) && reader.HasLineInfo());
        }

        public override string LookupNamespace(string ns)
        {
            return this.reader.LookupNamespace(ns);
        }

        public override void MoveToAttribute(int index)
        {
            this.OnEndOfContent();
            this.reader.MoveToAttribute(index);
        }

        public override bool MoveToAttribute(string name)
        {
            this.OnEndOfContent();
            return this.reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            this.OnEndOfContent();
            return this.reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            this.OnEndOfContent();
            return this.reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            this.OnEndOfContent();
            return this.reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            this.OnEndOfContent();
            return this.reader.MoveToNextAttribute();
        }

        private void OnEndOfContent()
        {
            if (this.contentReader != null)
            {
                this.contentReader.Close();
                this.contentReader = null;
            }
            if (this.contentStream != null)
            {
                this.contentStream.Close();
                this.contentStream = null;
            }
        }

        public override bool Read()
        {
            this.OnEndOfContent();
            if (!this.reader.Read())
            {
                return false;
            }
            if (!this.recordDone)
            {
                this.Record();
            }
            return true;
        }

        public override bool ReadAttributeValue()
        {
            return this.reader.ReadAttributeValue();
        }

        private int ReadBinaryContent(byte[] buffer, int offset, int count, bool isBase64)
        {
            CryptoHelper.ValidateBufferBounds(buffer, offset, count);
            int num = 0;
            while (((count > 0) && (this.NodeType != XmlNodeType.Element)) && (this.NodeType != XmlNodeType.EndElement))
            {
                if (this.contentStream == null)
                {
                    byte[] buffer2 = isBase64 ? Convert.FromBase64String(this.Value) : SoapHexBinary.Parse(this.Value).Value;
                    this.contentStream = new MemoryStream(buffer2);
                }
                int num2 = this.contentStream.Read(buffer, offset, count);
                if ((num2 == 0) && ((this.NodeType == XmlNodeType.Attribute) || !this.Read()))
                {
                    return num;
                }
                num += num2;
                offset += num2;
                count -= num2;
            }
            return num;
        }

        public override int ReadContentAsBase64(byte[] buffer, int offset, int count)
        {
            return this.ReadBinaryContent(buffer, offset, count, true);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int offset, int count)
        {
            return this.ReadBinaryContent(buffer, offset, count, false);
        }

        public override int ReadValueChunk(char[] chars, int offset, int count)
        {
            if (this.contentReader == null)
            {
                this.contentReader = new StringReader(this.Value);
            }
            return this.contentReader.Read(chars, offset, count);
        }

        private void Record()
        {
            switch (this.NodeType)
            {
                case XmlNodeType.Element:
                {
                    bool isEmptyElement = this.reader.IsEmptyElement;
                    this.xmlTokens.AddElement(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI, isEmptyElement);
                    if (this.reader.MoveToFirstAttribute())
                    {
                        do
                        {
                            this.xmlTokens.AddAttribute(this.reader.Prefix, this.reader.LocalName, this.reader.NamespaceURI, this.reader.Value);
                        }
                        while (this.reader.MoveToNextAttribute());
                        this.reader.MoveToElement();
                    }
                    if (!isEmptyElement)
                    {
                        this.depth++;
                        return;
                    }
                    if (this.depth == 0)
                    {
                        this.recordDone = true;
                    }
                    return;
                }
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.EntityReference:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.EndEntity:
                    this.xmlTokens.Add(this.NodeType, this.Value);
                    return;

                case XmlNodeType.DocumentType:
                case XmlNodeType.XmlDeclaration:
                    return;

                case XmlNodeType.EndElement:
                    this.xmlTokens.Add(this.NodeType, this.Value);
                    if (--this.depth == 0)
                    {
                        this.recordDone = true;
                    }
                    return;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.IdentityModel.SR.GetString("UnsupportedNodeTypeInReader", new object[] { this.reader.NodeType, this.reader.Name })));
        }

        public override void ResolveEntity()
        {
            this.reader.ResolveEntity();
        }

        public override int AttributeCount
        {
            get
            {
                return this.reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.reader.BaseURI;
            }
        }

        public override int Depth
        {
            get
            {
                return this.reader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.reader.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.reader.IsEmptyElement;
            }
        }

        public override string this[int index]
        {
            get
            {
                return this.reader[index];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.reader[name];
            }
        }

        public override string this[string name, string ns]
        {
            get
            {
                return this.reader[name, ns];
            }
        }

        public int LineNumber
        {
            get
            {
                IXmlLineInfo reader = this.reader as IXmlLineInfo;
                if (reader == null)
                {
                    return 1;
                }
                return reader.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                IXmlLineInfo reader = this.reader as IXmlLineInfo;
                if (reader == null)
                {
                    return 1;
                }
                return reader.LinePosition;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.reader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.reader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.reader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.reader.ReadState;
            }
        }

        public override string Value
        {
            get
            {
                return this.reader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.reader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.reader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.reader.XmlSpace;
            }
        }

        public XmlTokenStream XmlTokens
        {
            get
            {
                return this.xmlTokens;
            }
        }
    }
}

