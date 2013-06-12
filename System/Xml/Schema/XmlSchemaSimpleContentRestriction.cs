namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleContentRestriction : XmlSchemaContent
    {
        private XmlSchemaAnyAttribute anyAttribute;
        private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        private XmlSchemaSimpleType baseType;
        private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;
        private XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

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

        [XmlElement("simpleType", typeof(XmlSchemaSimpleType))]
        public XmlSchemaSimpleType BaseType
        {
            get
            {
                return this.baseType;
            }
            set
            {
                this.baseType = value;
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

        [XmlElement("pattern", typeof(XmlSchemaPatternFacet)), XmlElement("enumeration", typeof(XmlSchemaEnumerationFacet)), XmlElement("maxInclusive", typeof(XmlSchemaMaxInclusiveFacet)), XmlElement("minInclusive", typeof(XmlSchemaMinInclusiveFacet)), XmlElement("minExclusive", typeof(XmlSchemaMinExclusiveFacet)), XmlElement("length", typeof(XmlSchemaLengthFacet)), XmlElement("whiteSpace", typeof(XmlSchemaWhiteSpaceFacet)), XmlElement("maxLength", typeof(XmlSchemaMaxLengthFacet)), XmlElement("maxExclusive", typeof(XmlSchemaMaxExclusiveFacet)), XmlElement("minLength", typeof(XmlSchemaMinLengthFacet)), XmlElement("totalDigits", typeof(XmlSchemaTotalDigitsFacet)), XmlElement("fractionDigits", typeof(XmlSchemaFractionDigitsFacet))]
        public XmlSchemaObjectCollection Facets
        {
            get
            {
                return this.facets;
            }
        }
    }
}

