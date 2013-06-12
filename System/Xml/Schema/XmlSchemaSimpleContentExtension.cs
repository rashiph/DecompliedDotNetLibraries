namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleContentExtension : XmlSchemaContent
    {
        private XmlSchemaAnyAttribute anyAttribute;
        private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;

        internal void SetAttributes(XmlSchemaObjectCollection newAttributes)
        {
            this.attributes = newAttributes;
        }

        [XmlElement("anyAttribute")]
        public XmlSchemaAnyAttribute AnyAttribute
        {
            get
            {
                return this.anyAttribute;
            }
            set
            {
                this.anyAttribute = value;
            }
        }

        [XmlElement("attribute", typeof(XmlSchemaAttribute)), XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef))]
        public XmlSchemaObjectCollection Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        [XmlAttribute("base")]
        public XmlQualifiedName BaseTypeName
        {
            get
            {
                return this.baseTypeName;
            }
            set
            {
                this.baseTypeName = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }
    }
}

