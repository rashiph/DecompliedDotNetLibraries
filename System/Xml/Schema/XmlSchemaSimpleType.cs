namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleType : XmlSchemaType
    {
        private XmlSchemaSimpleTypeContent content;

        internal override XmlSchemaObject Clone()
        {
            XmlSchemaSimpleType type = (XmlSchemaSimpleType) base.MemberwiseClone();
            if (this.content != null)
            {
                type.Content = (XmlSchemaSimpleTypeContent) this.content.Clone();
            }
            return type;
        }

        [XmlElement("list", typeof(XmlSchemaSimpleTypeList)), XmlElement("restriction", typeof(XmlSchemaSimpleTypeRestriction)), XmlElement("union", typeof(XmlSchemaSimpleTypeUnion))]
        public XmlSchemaSimpleTypeContent Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        internal override XmlQualifiedName DerivedFrom
        {
            get
            {
                if ((this.content != null) && (this.content is XmlSchemaSimpleTypeRestriction))
                {
                    return ((XmlSchemaSimpleTypeRestriction) this.content).BaseTypeName;
                }
                return XmlQualifiedName.Empty;
            }
        }
    }
}

