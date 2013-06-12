namespace System.Xml.Schema
{
    internal class Datatype_tokenV1Compat : Datatype_normalizedStringV1Compat
    {
        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.Token;
            }
        }
    }
}

