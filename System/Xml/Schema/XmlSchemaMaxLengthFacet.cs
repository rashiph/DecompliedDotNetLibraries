namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMaxLengthFacet : XmlSchemaNumericFacet
    {
        public XmlSchemaMaxLengthFacet()
        {
            base.FacetType = FacetType.MaxLength;
        }
    }
}

