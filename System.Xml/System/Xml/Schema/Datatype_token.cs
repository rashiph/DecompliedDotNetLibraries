namespace System.Xml.Schema
{
    internal class Datatype_token : Datatype_normalizedString
    {
        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Collapse;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Token;
            }
        }
    }
}

