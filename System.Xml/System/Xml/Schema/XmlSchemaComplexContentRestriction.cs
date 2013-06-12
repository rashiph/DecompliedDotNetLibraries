namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaComplexContentRestriction : XmlSchemaContent
    {
        private XmlSchemaAnyAttribute anyAttribute;
        private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;
        private XmlSchemaParticle particle;

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

        [XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef)), XmlElement("attribute", typeof(XmlSchemaAttribute))]
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

        [XmlElement("choice", typeof(XmlSchemaChoice)), XmlElement("group", typeof(XmlSchemaGroupRef)), XmlElement("all", typeof(XmlSchemaAll)), XmlElement("sequence", typeof(XmlSchemaSequence))]
        public XmlSchemaParticle Particle
        {
            get
            {
                return this.particle;
            }
            set
            {
                this.particle = value;
            }
        }
    }
}

