namespace System.Xml.Schema
{
    using System;

    internal class Datatype_normalizedStringV1Compat : Datatype_string
    {
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

