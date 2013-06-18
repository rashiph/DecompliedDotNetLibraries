namespace System.Runtime.Serialization
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class XmlSerializableReader : XmlReader, IXmlLineInfo, IXmlTextParser
    {
        private XmlReader innerReader;
        private bool isRootEmptyElement;
        private int startDepth;
        private XmlReaderDelegator xmlReader;

        internal void BeginRead(XmlReaderDelegator xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializerReadContext.CreateUnexpectedStateException(XmlNodeType.Element, xmlReader));
            }
            this.xmlReader = xmlReader;
            this.startDepth = xmlReader.Depth;
            this.innerReader = xmlReader.UnderlyingReader;
            this.isRootEmptyElement = this.InnerReader.IsEmptyElement;
        }

        public override void Close()
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IXmlSerializableIllegalOperation")));
        }

        internal void EndRead()
        {
            if (this.isRootEmptyElement)
            {
                this.xmlReader.Read();
            }
            else
            {
                if (this.xmlReader.IsStartElement() && (this.xmlReader.Depth == this.startDepth))
                {
                    this.xmlReader.Read();
                }
                while (this.xmlReader.Depth > this.startDepth)
                {
                    if (!this.xmlReader.Read())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializerReadContext.CreateUnexpectedStateException(XmlNodeType.EndElement, this.xmlReader));
                    }
                }
            }
        }

        public override string GetAttribute(int i)
        {
            return this.InnerReader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this.InnerReader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this.InnerReader.GetAttribute(name, namespaceURI);
        }

        public override bool IsStartElement()
        {
            return this.InnerReader.IsStartElement();
        }

        public override bool IsStartElement(string name)
        {
            return this.InnerReader.IsStartElement(name);
        }

        public override bool IsStartElement(string localname, string ns)
        {
            return this.InnerReader.IsStartElement(localname, ns);
        }

        public override string LookupNamespace(string prefix)
        {
            return this.InnerReader.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            this.InnerReader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.InnerReader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this.InnerReader.MoveToAttribute(name, ns);
        }

        public override XmlNodeType MoveToContent()
        {
            return this.InnerReader.MoveToContent();
        }

        public override bool MoveToElement()
        {
            return this.InnerReader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.InnerReader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.InnerReader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            XmlReader innerReader = this.InnerReader;
            return (((innerReader.Depth != this.startDepth) || ((innerReader.NodeType != XmlNodeType.EndElement) && ((innerReader.NodeType != XmlNodeType.Element) || !innerReader.IsEmptyElement))) && innerReader.Read());
        }

        public override bool ReadAttributeValue()
        {
            return this.InnerReader.ReadAttributeValue();
        }

        public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
        {
            return this.InnerReader.ReadContentAs(returnType, namespaceResolver);
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            return this.InnerReader.ReadContentAsBase64(buffer, index, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            return this.InnerReader.ReadContentAsBinHex(buffer, index, count);
        }

        public override bool ReadContentAsBoolean()
        {
            return this.InnerReader.ReadContentAsBoolean();
        }

        public override DateTime ReadContentAsDateTime()
        {
            return this.InnerReader.ReadContentAsDateTime();
        }

        public override double ReadContentAsDouble()
        {
            return this.InnerReader.ReadContentAsDouble();
        }

        public override int ReadContentAsInt()
        {
            return this.InnerReader.ReadContentAsInt();
        }

        public override long ReadContentAsLong()
        {
            return this.InnerReader.ReadContentAsLong();
        }

        public override object ReadContentAsObject()
        {
            return this.InnerReader.ReadContentAsObject();
        }

        public override string ReadContentAsString()
        {
            return this.InnerReader.ReadContentAsString();
        }

        public override string ReadString()
        {
            return this.InnerReader.ReadString();
        }

        public override int ReadValueChunk(char[] buffer, int index, int count)
        {
            return this.InnerReader.ReadValueChunk(buffer, index, count);
        }

        public override void ResolveEntity()
        {
            this.InnerReader.ResolveEntity();
        }

        bool IXmlLineInfo.HasLineInfo()
        {
            IXmlLineInfo innerReader = this.InnerReader as IXmlLineInfo;
            if (innerReader != null)
            {
                return innerReader.HasLineInfo();
            }
            return this.xmlReader.HasLineInfo();
        }

        public override int AttributeCount
        {
            get
            {
                return this.InnerReader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.InnerReader.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return this.InnerReader.CanReadBinaryContent;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return this.InnerReader.CanReadValueChunk;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return this.InnerReader.CanResolveEntity;
            }
        }

        public override int Depth
        {
            get
            {
                return this.InnerReader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.InnerReader.EOF;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.InnerReader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.InnerReader.HasValue;
            }
        }

        private XmlReader InnerReader
        {
            get
            {
                return this.innerReader;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.InnerReader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.InnerReader.IsEmptyElement;
            }
        }

        public override string this[int i]
        {
            get
            {
                return this.InnerReader[i];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.InnerReader[name];
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                return this.InnerReader[name, namespaceURI];
            }
        }

        public override string LocalName
        {
            get
            {
                return this.InnerReader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.InnerReader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.InnerReader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.InnerReader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.InnerReader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.InnerReader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.InnerReader.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.InnerReader.ReadState;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this.InnerReader.SchemaInfo;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this.InnerReader.Settings;
            }
        }

        int IXmlLineInfo.LineNumber
        {
            get
            {
                IXmlLineInfo innerReader = this.InnerReader as IXmlLineInfo;
                if (innerReader != null)
                {
                    return innerReader.LineNumber;
                }
                return this.xmlReader.LineNumber;
            }
        }

        int IXmlLineInfo.LinePosition
        {
            get
            {
                IXmlLineInfo innerReader = this.InnerReader as IXmlLineInfo;
                if (innerReader != null)
                {
                    return innerReader.LinePosition;
                }
                return this.xmlReader.LinePosition;
            }
        }

        bool IXmlTextParser.Normalized
        {
            get
            {
                IXmlTextParser innerReader = this.InnerReader as IXmlTextParser;
                if (innerReader != null)
                {
                    return innerReader.Normalized;
                }
                return this.xmlReader.Normalized;
            }
            set
            {
                IXmlTextParser innerReader = this.InnerReader as IXmlTextParser;
                if (innerReader == null)
                {
                    this.xmlReader.Normalized = value;
                }
                else
                {
                    innerReader.Normalized = value;
                }
            }
        }

        WhitespaceHandling IXmlTextParser.WhitespaceHandling
        {
            get
            {
                IXmlTextParser innerReader = this.InnerReader as IXmlTextParser;
                if (innerReader != null)
                {
                    return innerReader.WhitespaceHandling;
                }
                return this.xmlReader.WhitespaceHandling;
            }
            set
            {
                IXmlTextParser innerReader = this.InnerReader as IXmlTextParser;
                if (innerReader == null)
                {
                    this.xmlReader.WhitespaceHandling = value;
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
                return this.InnerReader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.InnerReader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.InnerReader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.InnerReader.XmlSpace;
            }
        }
    }
}

