namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaPatternFacet : XmlSchemaFacet
    {
        public XmlSchemaPatternFacet()
        {
            base.FacetType = FacetType.Pattern;
        }
    }
}

