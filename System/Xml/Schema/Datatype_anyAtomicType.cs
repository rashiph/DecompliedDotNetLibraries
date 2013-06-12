namespace System.Xml.Schema
{
    internal class Datatype_anyAtomicType : Datatype_anySimpleType
    {
        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlAnyConverter.AnyAtomic;
        }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Preserve;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.AnyAtomicType;
            }
        }
    }
}

