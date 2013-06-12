namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMinLengthFacet : XmlSchemaNumericFacet
    {
        public XmlSchemaMinLengthFacet()
        {
            base.FacetType = FacetType.MinLength;
        }
    }
}

