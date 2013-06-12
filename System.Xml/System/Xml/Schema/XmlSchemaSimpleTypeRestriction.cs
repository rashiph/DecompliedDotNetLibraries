namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleTypeRestriction : XmlSchemaSimpleTypeContent
    {
        private XmlSchemaSimpleType baseType;
        private XmlQualifiedName baseTypeName = XmlQualifiedName.Empty;
        private XmlSchemaObjectCollection facets = new XmlSchemaObjectCollection();

        internal override XmlSchemaObject Clone()
        {
            XmlSchemaSimpleTypeRestriction restriction = (XmlSchemaSimpleTypeRestriction) base.MemberwiseClone();
            restriction.BaseTypeName = this.baseTypeName.Clone();
            return restriction;
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

        [XmlElement("maxLength", typeof(XmlSchemaMaxLengthFacet)), XmlElement("maxInclusive", typeof(XmlSchemaMaxInclusiveFacet)), XmlElement("minInclusive", typeof(XmlSchemaMinInclusiveFacet)), XmlElement("length", typeof(XmlSchemaLengthFacet)), XmlElement("minLength", typeof(XmlSchemaMinLengthFacet)), XmlElement("pattern", typeof(XmlSchemaPatternFacet)), XmlElement("enumeration", typeof(XmlSchemaEnumerationFacet)), XmlElement("maxExclusive", typeof(XmlSchemaMaxExclusiveFacet)), XmlElement("fractionDigits", typeof(XmlSchemaFractionDigitsFacet)), XmlElement("minExclusive", typeof(XmlSchemaMinExclusiveFacet)), XmlElement("totalDigits", typeof(XmlSchemaTotalDigitsFacet)), XmlElement("whiteSpace", typeof(XmlSchemaWhiteSpaceFacet))]
        public XmlSchemaObjectCollection Facets
        {
            get
            {
                return this.facets;
            }
        }
    }
}

