namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaFractionDigitsFacet : XmlSchemaNumericFacet
    {
        public XmlSchemaFractionDigitsFacet()
        {
            base.FacetType = FacetType.FractionDigits;
        }
    }
}

