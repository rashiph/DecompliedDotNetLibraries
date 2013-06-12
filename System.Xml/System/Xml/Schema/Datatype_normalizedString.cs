namespace System.Xml.Schema
{
    using System;

    internal class Datatype_normalizedString : Datatype_string
    {
        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Replace;
            }
        }

        internal override bool HasValueFacets
        {
            get
            {
                return true;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.NormalizedString;
            }
        }
    }
}

