namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public abstract class XmlSchemaExternal : XmlSchemaObject
    {
        private Uri baseUri;
        private System.Xml.Schema.Compositor compositor;
        private string id;
        private string location;
        private XmlAttribute[] moreAttributes;
        private XmlSchema schema;

        protected XmlSchemaExternal()
        {
        }

        internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
        {
            this.moreAttributes = moreAttributes;
        }

        [XmlIgnore]
        internal Uri BaseUri
        {
            get
            {
                return this.baseUri;
            }
            set
            {
                this.baseUri = value;
            }
        }

        internal System.Xml.Schema.Compositor Compositor
        {
            get
            {
                return this.compositor;
            }
            set
            {
                this.compositor = value;
            }
        }

        [XmlAttribute("id", DataType="ID")]
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        [XmlIgnore]
        internal override string IdAttribute
        {
            get
            {
                return this.Id;
            }
            set
            {
                this.Id = value;
            }
        }

        [XmlIgnore]
        public XmlSchema Schema
        {
            get
            {
                return this.schema;
            }
            set
            {
                this.schema = value;
            }
        }

        [XmlAttribute("schemaLocation", DataType="anyURI")]
        public string SchemaLocation
        {
            get
            {
                return this.location;
            }
            set
            {
                this.location = value;
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] UnhandledAttributes
        {
            get
            {
                return this.moreAttributes;
            }
            set
            {
                this.moreAttributes = value;
            }
        }
    }
}

