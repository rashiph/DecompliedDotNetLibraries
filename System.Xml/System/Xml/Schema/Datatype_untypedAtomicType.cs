namespace System.Xml.Schema
{
    internal class Datatype_untypedAtomicType : Datatype_anyAtomicType
    {
        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return XmlUntypedConverter.Untyped;
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
                return XmlTypeCode.UntypedAtomic;
            }
        }
    }
}

