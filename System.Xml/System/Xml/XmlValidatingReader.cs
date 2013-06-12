namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml.Schema;

    [Obsolete("Use XmlReader created by XmlReader.Create() method using appropriate XmlReaderSettings instead. http://go.microsoft.com/fwlink/?linkid=14202"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class XmlValidatingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        private XmlValidatingReaderImpl impl;

        public event System.Xml.Schema.ValidationEventHandler ValidationEventHandler
        {
            add
            {
                this.impl.ValidationEventHandler += value;
            }
            remove
            {
                this.impl.ValidationEventHandler -= value;
            }
        }

        public XmlValidatingReader(XmlReader reader)
        {
            this.impl = new XmlValidatingReaderImpl(reader);
            this.impl.OuterReader = this;
        }

        public XmlValidatingReader(Stream xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            if (xmlFragment == null)
            {
                throw new ArgumentNullException("xmlFragment");
            }
            this.impl = new XmlValidatingReaderImpl(xmlFragment, fragType, context);
            this.impl.OuterReader = this;
        }

        public XmlValidatingReader(string xmlFragment, XmlNodeType fragType, XmlParserContext context)
        {
            if (xmlFragment == null)
            {
                throw new ArgumentNullException("xmlFragment");
            }
            this.impl = new XmlValidatingReaderImpl(xmlFragment, fragType, context);
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

        public object ReadTypedValue()
        {
            return this.impl.ReadTypedValue();
        }

        public override void ResolveEntity()
        {
            this.impl.ResolveEntity();
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

        internal XmlValidatingReaderImpl Impl
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

        public override string Prefix
        {
            get
            {
                return this.impl.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this.impl.QuoteChar;
            }
        }

        public XmlReader Reader
        {
            get
            {
                return this.impl.Reader;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this.impl.ReadState;
            }
        }

        public XmlSchemaCollection Schemas
        {
            get
            {
                return this.impl.Schemas;
            }
        }

        public object SchemaType
        {
            get
            {
                return this.impl.SchemaType;
            }
        }

        public System.Xml.ValidationType ValidationType
        {
            get
            {
                return this.impl.ValidationType;
            }
            set
            {
                this.impl.ValidationType = value;
            }
        }

        public override string Value
        {
            get
            {
                return this.impl.Value;
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
    }
}

