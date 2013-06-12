namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaInfo : IXmlSchemaInfo
    {
        private XmlSchemaContentType contentType;
        private bool isDefault;
        private bool isNil;
        private XmlSchemaSimpleType memberType;
        private XmlSchemaAttribute schemaAttribute;
        private XmlSchemaElement schemaElement;
        private XmlSchemaType schemaType;
        private XmlSchemaValidity validity;

        public XmlSchemaInfo()
        {
            this.Clear();
        }

        internal XmlSchemaInfo(XmlSchemaValidity validity) : this()
        {
            this.validity = validity;
        }

        internal void Clear()
        {
            this.isNil = false;
            this.isDefault = false;
            this.schemaType = null;
            this.schemaElement = null;
            this.schemaAttribute = null;
            this.memberType = null;
            this.validity = XmlSchemaValidity.NotKnown;
            this.contentType = XmlSchemaContentType.Empty;
        }

        public XmlSchemaContentType ContentType
        {
            get
            {
                return this.contentType;
            }
            set
            {
                this.contentType = value;
            }
        }

        internal bool HasDefaultValue
        {
            get
            {
                return ((this.schemaElement != null) && (this.schemaElement.ElementDecl.DefaultValueTyped != null));
            }
        }

        public bool IsDefault
        {
            get
            {
                return this.isDefault;
            }
            set
            {
                this.isDefault = value;
            }
        }

        public bool IsNil
        {
            get
            {
                return this.isNil;
            }
            set
            {
                this.isNil = value;
            }
        }

        internal bool IsUnionType
        {
            get
            {
                return (((this.schemaType != null) && (this.schemaType.Datatype != null)) && (this.schemaType.Datatype.Variety == XmlSchemaDatatypeVariety.Union));
            }
        }

        public XmlSchemaSimpleType MemberType
        {
            get
            {
                return this.memberType;
            }
            set
            {
                this.memberType = value;
            }
        }

        public XmlSchemaAttribute SchemaAttribute
        {
            get
            {
                return this.schemaAttribute;
            }
            set
            {
                this.schemaAttribute = value;
                if (value != null)
                {
                    this.schemaElement = null;
                }
            }
        }

        public XmlSchemaElement SchemaElement
        {
            get
            {
                return this.schemaElement;
            }
            set
            {
                this.schemaElement = value;
                if (value != null)
                {
                    this.schemaAttribute = null;
                }
            }
        }

        public XmlSchemaType SchemaType
        {
            get
            {
                return this.schemaType;
            }
            set
            {
                this.schemaType = value;
                if (this.schemaType != null)
                {
                    this.contentType = this.schemaType.SchemaContentType;
                }
                else
                {
                    this.contentType = XmlSchemaContentType.Empty;
                }
            }
        }

        public XmlSchemaValidity Validity
        {
            get
            {
                return this.validity;
            }
            set
            {
                this.validity = value;
            }
        }

        internal XmlSchemaType XmlType
        {
            get
            {
                if (this.memberType != null)
                {
                    return this.memberType;
                }
                return this.schemaType;
            }
        }
    }
}

