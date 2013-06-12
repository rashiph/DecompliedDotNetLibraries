namespace System.Xml
{
    using System;
    using System.Xml.Schema;

    public class XmlDocumentType : XmlLinkedNode
    {
        private XmlNamedNodeMap entities;
        private string internalSubset;
        private string name;
        private bool namespaces;
        private XmlNamedNodeMap notations;
        private string publicId;
        private SchemaInfo schemaInfo;
        private string systemId;

        protected internal XmlDocumentType(string name, string publicId, string systemId, string internalSubset, XmlDocument doc) : base(doc)
        {
            this.name = name;
            this.publicId = publicId;
            this.systemId = systemId;
            this.namespaces = true;
            this.internalSubset = internalSubset;
            if (!doc.IsLoading)
            {
                doc.IsLoading = true;
                new XmlLoader().ParseDocumentType(this);
                doc.IsLoading = false;
            }
        }

        public override XmlNode CloneNode(bool deep)
        {
            return this.OwnerDocument.CreateDocumentType(this.name, this.publicId, this.systemId, this.internalSubset);
        }

        public override void WriteContentTo(XmlWriter w)
        {
        }

        public override void WriteTo(XmlWriter w)
        {
            w.WriteDocType(this.name, this.publicId, this.systemId, this.internalSubset);
        }

        internal SchemaInfo DtdSchemaInfo
        {
            get
            {
                return this.schemaInfo;
            }
            set
            {
                this.schemaInfo = value;
            }
        }

        public XmlNamedNodeMap Entities
        {
            get
            {
                if (this.entities == null)
                {
                    this.entities = new XmlNamedNodeMap(this);
                }
                return this.entities;
            }
        }

        public string InternalSubset
        {
            get
            {
                return this.internalSubset;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.name;
            }
        }

        public override string Name
        {
            get
            {
                return this.name;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.DocumentType;
            }
        }

        public XmlNamedNodeMap Notations
        {
            get
            {
                if (this.notations == null)
                {
                    this.notations = new XmlNamedNodeMap(this);
                }
                return this.notations;
            }
        }

        internal bool ParseWithNamespaces
        {
            get
            {
                return this.namespaces;
            }
            set
            {
                this.namespaces = value;
            }
        }

        public string PublicId
        {
            get
            {
                return this.publicId;
            }
        }

        public string SystemId
        {
            get
            {
                return this.systemId;
            }
        }
    }
}

