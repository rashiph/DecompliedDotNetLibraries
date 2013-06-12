namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class XmlTextReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private XmlTextReaderImpl impl;

        protected XmlTextReader()
        {
            this.impl = new XmlTextReaderImpl();
            this.impl.OuterReader = this;
        }

        public XmlTextReader(Stream input)
        {
            this.impl = new XmlTextReaderImpl(input);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(TextReader input)
        {
            this.impl = new XmlTextReaderImpl(input);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url)
        {
            this.impl = new XmlTextReaderImpl(url, new System.Xml.NameTable());
            this.impl.OuterReader = this;
        }

        protected XmlTextReader(XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(Stream input, XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(input, nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(TextReader input, XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(input, nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url, Stream input)
        {
            this.impl = new XmlTextReaderImpl(url, input);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url, TextReader input)
        {
            this.impl = new XmlTextReaderImpl(url, input);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url, XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(url, nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            this.impl = new XmlTextReaderImpl(xmlFragment, fragType, context);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url, Stream input, XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(url, input, nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string url, TextReader input, XmlNameTable nt)
        {
            this.impl = new XmlTextReaderImpl(url, input, nt);
            this.impl.OuterReader = this;
        }

        public XmlTextReader(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            this.impl = new XmlTextReaderImpl(xmlFragment, fragType, context);
            this.impl.OuterReader = this;
        }

        public override void Close()
        {
            this.impl.Close();
        }

        public override string GetAttribute(int i)
        {
            return this.impl.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this.impl.GetAttribute(name);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            return this.impl.GetAttribute(localName, namespaceURI);
        }

        public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.impl.GetNamespacesInScope(scope);
        }

        public TextReader GetRemainder()
        {
            return this.impl.GetRemainder();
        }

        public bool HasLineInfo()
        {
            return true;
        }

        public override string LookupNamespace(string prefix)
        {
            string str = this.impl.LookupNamespace(prefix);
            if ((str != null) && (str.Length == 0))
            {
                str = null;
            }
            return str;
        }

        public override void MoveToAttribute(int i)
        {
            this.impl.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this.impl.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            return this.impl.MoveToAttribute(localName, namespaceURI);
        }

        public override bool MoveToElement()
        {
            return this.impl.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this.impl.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this.impl.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return this.impl.Read();
        }

        public override bool ReadAttributeValue()
        {
            return this.impl.ReadAttributeValue();
        }

        public int ReadBase64(byte[] array, int offset, int len)
        {
            return this.impl.ReadBase64(array, offset, len);
        }

        public int ReadBinHex(byte[] array, int offset, int len)
        {
            return this.impl.ReadBinHex(array, offset, len);
        }

        public int ReadChars(char[] buffer, int index, int count)
        {
            return this.impl.ReadChars(buffer, index, count);
        }

        public override int ReadContentAsBase64(byte[] buffer, int index, int count)
        {
            return this.impl.ReadContentAsBase64(buffer, index, count);
        }

        public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
        {
            return this.impl.ReadContentAsBinHex(buffer, index, count);
        }

        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
        {
            return this.impl.ReadElementContentAsBase64(buffer, index, count);
        }

        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
        {
            return this.impl.ReadElementContentAsBinHex(buffer, index, count);
        }

        public override string ReadString()
        {
            this.impl.MoveOffEntityReference();
            return base.ReadString();
        }

        public void ResetState()
        {
            this.impl.ResetState();
        }

        public override void ResolveEntity()
        {
            this.impl.ResolveEntity();
        }

        public override void Skip()
        {
            this.impl.Skip();
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            return this.impl.GetNamespacesInScope(scope);
        }

        string IXmlNamespaceResolver.LookupNamespace(string prefix)
        {
            return this.impl.LookupNamespace(prefix);
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            return this.impl.LookupPrefix(namespaceName);
        }

        public override int AttributeCount
        {
            get
            {
                return this.impl.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this.impl.BaseURI;
            }
        }

        public override bool CanReadBinaryContent
        {
            get
            {
                return true;
            }
        }

        public override bool CanReadValueChunk
        {
            get
            {
                return false;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return true;
            }
        }

        public override int Depth
        {
            get
            {
                return this.impl.Depth;
            }
        }

        internal override IDtdInfo DtdInfo
        {
            get
            {
                return this.impl.DtdInfo;
            }
        }

        public System.Xml.DtdProcessing DtdProcessing
        {
            get
            {
                return this.impl.DtdProcessing;
            }
            set
            {
                this.impl.DtdProcessing = value;
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.impl.Encoding;
            }
        }

        public System.Xml.EntityHandling EntityHandling
        {
            get
            {
                return this.impl.EntityHandling;
            }
            set
            {
                this.impl.EntityHandling = value;
            }
        }

        public override bool EOF
        {
            get
            {
                return this.impl.EOF;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this.impl.HasValue;
            }
        }

        internal XmlTextReaderImpl Impl
        {
            get
            {
                return this.impl;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this.impl.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.impl.IsEmptyElement;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.impl.LineNumber;
            }
        }

        public int LinePosition
        {
            get
            {
                return this.impl.LinePosition;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.impl.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.impl.Name;
            }
        }

        internal override XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this.impl.NamespaceManager;
            }
        }

        public bool Namespaces
        {
            get
            {
                return this.impl.Namespaces;
            }
            set
            {
                this.impl.Namespaces = value;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.impl.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.impl.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this.impl.NodeType;
            }
        }

        public bool Normalization
        {
            get
            {
                return this.impl.Normalization;
            }
            set
            {
                this.impl.Normalization = value;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.impl.Prefix;
            }
        }

        [Obsolete("Use DtdProcessing property instead.")]
        public bool ProhibitDtd
        {
            get
            {
                return (this.impl.DtdProcessing == System.Xml.DtdProcessing.Prohibit);
            }
            set
            {
                this.impl.DtdProcessing = value ? System.Xml.DtdProcessing.Prohibit : System.Xml.DtdProcessing.Parse;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.impl.QuoteChar;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.impl.ReadState;
            }
        }

        public override string Value
        {
            get
            {
                return this.impl.Value;
            }
        }

        public System.Xml.WhitespaceHandling WhitespaceHandling
        {
            get
            {
                return this.impl.WhitespaceHandling;
            }
            set
            {
                this.impl.WhitespaceHandling = value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.impl.XmlLang;
            }
        }

        public System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.impl.XmlResolver = value;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this.impl.XmlSpace;
            }
        }

        internal bool XmlValidatingReaderCompatibilityMode
        {
            set
            {
                this.impl.XmlValidatingReaderCompatibilityMode = value;
            }
        }
    }
}

