namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaTotalDigitsFacet : XmlSchemaNumericFacet
    {
        public XmlSchemaTotalDigitsFacet()
        {
            base.FacetType = FacetType.TotalDigits;
        }
    }
}

