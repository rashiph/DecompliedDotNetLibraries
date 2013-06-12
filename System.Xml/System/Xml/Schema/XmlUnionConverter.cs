namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal class XmlUnionConverter : XmlBaseConverter
    {
        private XmlValueConverter[] converters;
        private bool hasAtomicMember;
        private bool hasListMember;

        protected XmlUnionConverter(XmlSchemaType schemaType) : base(schemaType)
        {
            while (schemaType.DerivedBy == XmlSchemaDerivationMethod.Restriction)
            {
                schemaType = schemaType.BaseXmlSchemaType;
            }
            XmlSchemaSimpleType[] baseMemberTypes = ((XmlSchemaSimpleTypeUnion) ((XmlSchemaSimpleType) schemaType).Content).BaseMemberTypes;
            this.converters = new XmlValueConverter[baseMemberTypes.Length];
            for (int i = 0; i < baseMemberTypes.Length; i++)
            {
                this.converters[i] = baseMemberTypes[i].ValueConverter;
                if (baseMemberTypes[i].Datatype.Variety == XmlSchemaDatatypeVariety.List)
                {
                    this.hasListMember = true;
                }
                else if (baseMemberTypes[i].Datatype.Variety == XmlSchemaDatatypeVariety.Atomic)
                {
                    this.hasAtomicMember = true;
                }
            }
        }

        public override object ChangeType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            Type sourceType = value.GetType();
            if ((sourceType == XmlBaseConverter.XmlAtomicValueType) && this.hasAtomicMember)
            {
                return ((XmlAtomicValue) value).ValueAs(destinationType, nsResolver);
            }
            if ((sourceType == XmlBaseConverter.XmlAtomicValueArrayType) && this.hasListMember)
            {
                return XmlAnyListConverter.ItemList.ChangeType(value, destinationType, nsResolver);
            }
            if (!(sourceType == XmlBaseConverter.StringType))
            {
                throw base.CreateInvalidClrMappingException(sourceType, destinationType);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                return value;
            }
            XsdSimpleValue value2 = (XsdSimpleValue) base.SchemaType.Datatype.ParseValue((string) value, new NameTable(), nsResolver, true);
            return value2.XmlType.ValueConverter.ChangeType((string) value, destinationType, nsResolver);
        }

        public static XmlValueConverter Create(XmlSchemaType schemaType)
        {
            return new XmlUnionConverter(schemaType);
        }
    }
}

