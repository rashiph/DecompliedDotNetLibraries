namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlCountingReader : XmlReader, IXmlTextParser, IXmlLineInfo
    {
        private int advanceCount;
        private XmlReader innerReader;

        internal XmlCountingReader(XmlReader xmlReader)
        {
            if (xmlReader == null)
            {
                throw new ArgumentNullException("xmlReader");
            }
            this.innerReader = xmlReader;
            this.advanceCount = 0;
        }

        public override void Close()
        {
            this.innerReader.Close();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    IDisposable innerReader = this.innerReader;
                    if (innerReader != null)
                    {
                        innerReader.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override string GetAttribute(int i)
        {
            return this.innerReader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this.innerReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.innerReader.GetAttribute(name, namespaceURI);
        }

        private void IncrementCount()
        {
            if (this.advanceCount == 0x7fffffff)
            {
                this.advanceCount = 0;
            }
            else
            {
                this.advanceCount++;
            }
        }

        public override bool IsStartElement()
        {
            return this.innerReader.IsStartElement();
        }

        public override bool IsStartElement(string name)
        {
            return this.innerReader.IsStartElement(name);
        }

        public override bool IsStartElement(string localname, string ns)
        {
            return this.innerReader.IsStartElement(localname, ns);
        }

        public override string LookupNamespace(string prefix)
        {
            return this.innerReader.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            this.innerReader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.innerReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.innerReader.MoveToAttribute(name, ns);
        }

        public override XmlNodeType MoveToContent()
        {
            return this.innerReader.MoveToContent();
        }

        public override bool MoveToElement()
        {
            return this.innerReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.innerReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.innerReader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            this.IncrementCount();
            return this.innerReader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return this.innerReader.ReadAttributeValue();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAs(returnType, namespaceResolver);
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsBase64(buffer, index, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsBinHex(buffer, index, count);
        }

        public override bool ReadContentAsBoolean()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsDateTime();
        }

        public override double ReadContentAsDouble()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsDouble();
        }

        public override int ReadContentAsInt()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsLong();
        }

        public override object ReadContentAsObject()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsObject();
        }

        public override string ReadContentAsString()
        {
            this.IncrementCount();
            return this.innerReader.ReadContentAsString();
        }

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAs(returnType, namespaceResolver);
        }

        public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsBase64(buffer, index, count);
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsBinHex(buffer, index, count);
        }

        public override bool ReadElementContentAsBoolean()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsBoolean();
        }

        public override bool ReadElementContentAsBoolean(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsBoolean(localName, namespaceURI);
        }

        public override DateTime ReadElementContentAsDateTime()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsDateTime();
        }

        public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsDateTime(localName, namespaceURI);
        }

        public override double ReadElementContentAsDouble()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsDouble();
        }

        public override double ReadElementContentAsDouble(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsDouble(localName, namespaceURI);
        }

        public override int ReadElementContentAsInt()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsInt();
        }

        public override int ReadElementContentAsInt(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsInt(localName, namespaceURI);
        }

        public override long ReadElementContentAsLong()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsLong();
        }

        public override long ReadElementContentAsLong(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsLong(localName, namespaceURI);
        }

        public override object ReadElementContentAsObject()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsObject();
        }

        public override object ReadElementContentAsObject(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsObject(localName, namespaceURI);
        }

        public override string ReadElementContentAsString()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsString();
        }

        public override string ReadElementContentAsString(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementContentAsString(localName, namespaceURI);
        }

        public override string ReadElementString()
        {
            this.IncrementCount();
            return this.innerReader.ReadElementString();
        }

        public override string ReadElementString(string name)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementString(name);
        }

        public override string ReadElementString(string localname, string ns)
        {
            this.IncrementCount();
            return this.innerReader.ReadElementString(localname, ns);
        }

        public override void ReadEndElement()
        {
            this.IncrementCount();
            this.innerReader.ReadEndElement();
        }

        public override string ReadInnerXml()
        {
            if (this.innerReader.NodeType != XmlNodeType.Attribute)
            {
                this.IncrementCount();
            }
            return this.innerReader.ReadInnerXml();
        }

        public override string ReadOuterXml()
        {
            if (this.innerReader.NodeType != XmlNodeType.Attribute)
            {
                this.IncrementCount();
            }
            return this.innerReader.ReadOuterXml();
        }

        public override void ReadStartElement()
        {
            this.IncrementCount();
            this.innerReader.ReadStartElement();
        }

        public override void ReadStartElement(string name)
        {
            this.IncrementCount();
            this.innerReader.ReadStartElement(name);
        }

        public override void ReadStartElement(string localname, string ns)
        {
            this.IncrementCount();
            this.innerReader.ReadStartElement(localname, ns);
        }

        public override string ReadString()
        {
            this.IncrementCount();
            return this.innerReader.ReadString();
        }

        public override XmlReader ReadSubtree()
        {
            return this.innerReader.ReadSubtree();
        }

        public override bool ReadToDescendant(string name)
        {
            this.IncrementCount();
            return this.innerReader.ReadToDescendant(name);
        }

        public override bool ReadToDescendant(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadToDescendant(localName, namespaceURI);
        }

        public override bool ReadToFollowing(string name)
        {
            this.IncrementCount();
            return this.ReadToFollowing(name);
        }

        public override bool ReadToFollowing(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadToFollowing(localName, namespaceURI);
        }

        public override bool ReadToNextSibling(string name)
        {
            this.IncrementCount();
            return this.innerReader.ReadToNextSibling(name);
        }

        public override bool ReadToNextSibling(string localName, string namespaceURI)
        {
            this.IncrementCount();
            return this.innerReader.ReadToNextSibling(localName, namespaceURI);
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            this.IncrementCount();
            return this.innerReader.ReadValueChunk(buffer, index, count);
        }

        public override void ResolveEntity()
        {
            this.innerReader.ResolveEntity();
        }

        public override void Skip()
        {
            this.IncrementCount();
            this.innerReader.Skip();
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            IXmlLineInfo innerReader = this.innerReader as IXmlLineInfo;
            return ((innerReader != null) && innerReader.HasLineInfo());
        }

        internal int AdvanceCount
        {
            get
            {
                return this.advanceCount;
            }
        }

        public override int AttributeCount
        {
            get
            {
                return this.innerReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.innerReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return this.innerReader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return this.innerReader.CanReadValueChunk;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return this.innerReader.CanResolveEntity;
            }
        }

        public override int Depth
        {
            get
            {
                return this.innerReader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.innerReader.EOF;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.innerReader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.innerReader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.innerReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.innerReader.IsEmptyElement;
            }
        }

        public override string this[int i]
        {
            get
            {
                return this.innerReader[i];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.innerReader[name];
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                return this.innerReader[name, namespaceURI];
            }
        }

        public override string LocalName
        {
            get
            {
                return this.innerReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.innerReader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.innerReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.innerReader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.innerReader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.innerReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.innerReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.innerReader.ReadState;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.innerReader.SchemaInfo;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this.innerReader.Settings;
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                IXmlLineInfo innerReader = this.innerReader as IXmlLineInfo;
                if (innerReader != null)
                {
                    return innerReader.LineNumber;
                }
                return 0;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                IXmlLineInfo innerReader = this.innerReader as IXmlLineInfo;
                if (innerReader != null)
                {
                    return innerReader.LinePosition;
                }
                return 0;
            }
        }

        bool IXmlTextParser.Normalized
        {
            get
            {
                XmlTextReader innerReader = this.innerReader as XmlTextReader;
                if (innerReader != null)
                {
                    return innerReader.Normalization;
                }
                IXmlTextParser parser = this.innerReader as IXmlTextParser;
                return ((parser != null) && parser.Normalized);
            }
            set
            {
                XmlTextReader innerReader = this.innerReader as XmlTextReader;
                if (innerReader == null)
                {
                    IXmlTextParser parser = this.innerReader as IXmlTextParser;
                    if (parser != null)
                    {
                        parser.Normalized = value;
                    }
                }
                else
                {
                    innerReader.Normalization = value;
                }
            }
        }

        WhitespaceHandling IXmlTextParser.WhitespaceHandling
        {
            get
            {
                XmlTextReader innerReader = this.innerReader as XmlTextReader;
                if (innerReader != null)
                {
                    return innerReader.WhitespaceHandling;
                }
                IXmlTextParser parser = this.innerReader as IXmlTextParser;
                if (parser != null)
                {
                    return parser.WhitespaceHandling;
                }
                return WhitespaceHandling.None;
            }
            set
            {
                XmlTextReader innerReader = this.innerReader as XmlTextReader;
                if (innerReader == null)
                {
                    IXmlTextParser parser = this.innerReader as IXmlTextParser;
                    if (parser != null)
                    {
                        parser.WhitespaceHandling = value;
                    }
                }
                else
                {
                    innerReader.WhitespaceHandling = value;
                }
            }
        }

        public override string Value
        {
            get
            {
                return this.innerReader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.innerReader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.innerReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.innerReader.XmlSpace;
            }
        }
    }
}

