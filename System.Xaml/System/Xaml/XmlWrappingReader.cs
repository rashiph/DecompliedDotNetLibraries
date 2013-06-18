namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Schema;

    internal class XmlWrappingReader : XmlReader, IXmlLineInfo, IXmlNamespaceResolver
    {
        protected XmlReader _reader;
        protected IXmlLineInfo _readerAsIXmlLineInfo;
        protected IXmlNamespaceResolver _readerAsResolver;

        internal XmlWrappingReader(XmlReader baseReader)
        {
            this.Reader = baseReader;
        }

        public override void Close()
        {
            this._reader.Close();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this._reader.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override string GetAttribute(int i)
        {
            return this._reader.GetAttribute(i);
        }

        public override string GetAttribute(string name)
        {
            return this._reader.GetAttribute(name);
        }

        public override string GetAttribute(string name, string namespaceURI)
        {
            return this._reader.GetAttribute(name, namespaceURI);
        }

        public virtual bool HasLineInfo()
        {
            return ((this._readerAsIXmlLineInfo != null) && this._readerAsIXmlLineInfo.HasLineInfo());
        }

        public override string LookupNamespace(string prefix)
        {
            return this._reader.LookupNamespace(prefix);
        }

        public override void MoveToAttribute(int i)
        {
            this._reader.MoveToAttribute(i);
        }

        public override bool MoveToAttribute(string name)
        {
            return this._reader.MoveToAttribute(name);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return this._reader.MoveToAttribute(name, ns);
        }

        public override bool MoveToElement()
        {
            return this._reader.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return this._reader.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return this._reader.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return this._reader.Read();
        }

        public override bool ReadAttributeValue()
        {
            return this._reader.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            this._reader.ResolveEntity();
        }

        public override void Skip()
        {
            this._reader.Skip();
        }

        IDictionary<string, string> IXmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope scope)
        {
            if (this._readerAsResolver != null)
            {
                return this._readerAsResolver.GetNamespacesInScope(scope);
            }
            return null;
        }

        string IXmlNamespaceResolver.LookupPrefix(string namespaceName)
        {
            if (this._readerAsResolver != null)
            {
                return this._readerAsResolver.LookupPrefix(namespaceName);
            }
            return null;
        }

        public override int AttributeCount
        {
            get
            {
                return this._reader.AttributeCount;
            }
        }

        public override string BaseURI
        {
            get
            {
                return this._reader.BaseURI;
            }
        }

        public override bool CanResolveEntity
        {
            get
            {
                return this._reader.CanResolveEntity;
            }
        }

        public override int Depth
        {
            get
            {
                return this._reader.Depth;
            }
        }

        public override bool EOF
        {
            get
            {
                return this._reader.EOF;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this._reader.HasAttributes;
            }
        }

        public override bool HasValue
        {
            get
            {
                return this._reader.HasValue;
            }
        }

        public override bool IsDefault
        {
            get
            {
                return this._reader.IsDefault;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this._reader.IsEmptyElement;
            }
        }

        public override string this[int i]
        {
            get
            {
                return this._reader[i];
            }
        }

        public override string this[string name]
        {
            get
            {
                return this._reader[name];
            }
        }

        public override string this[string name, string namespaceURI]
        {
            get
            {
                return this._reader[name, namespaceURI];
            }
        }

        public virtual int LineNumber
        {
            get
            {
                if (this._readerAsIXmlLineInfo != null)
                {
                    return this._readerAsIXmlLineInfo.LineNumber;
                }
                return 0;
            }
        }

        public virtual int LinePosition
        {
            get
            {
                if (this._readerAsIXmlLineInfo != null)
                {
                    return this._readerAsIXmlLineInfo.LinePosition;
                }
                return 0;
            }
        }

        public override string LocalName
        {
            get
            {
                return this._reader.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this._reader.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this._reader.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this._reader.NameTable;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return this._reader.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this._reader.Prefix;
            }
        }

        public override char QuoteChar
        {
            get
            {
                return this._reader.QuoteChar;
            }
        }

        protected XmlReader Reader
        {
            get
            {
                return this._reader;
            }
            set
            {
                this._reader = value;
                this._readerAsIXmlLineInfo = value as IXmlLineInfo;
                this._readerAsResolver = value as IXmlNamespaceResolver;
            }
        }

        public override System.Xml.ReadState ReadState
        {
            get
            {
                return this._reader.ReadState;
            }
        }

        public override IXmlSchemaInfo SchemaInfo
        {
            get
            {
                return this._reader.SchemaInfo;
            }
        }

        public override XmlReaderSettings Settings
        {
            get
            {
                return this._reader.Settings;
            }
        }

        public override string Value
        {
            get
            {
                return this._reader.Value;
            }
        }

        public override Type ValueType
        {
            get
            {
                return this._reader.ValueType;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this._reader.XmlLang;
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this._reader.XmlSpace;
            }
        }
    }
}

