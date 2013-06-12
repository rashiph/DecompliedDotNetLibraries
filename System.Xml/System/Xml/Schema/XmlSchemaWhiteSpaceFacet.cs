namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaWhiteSpaceFacet : XmlSchemaFacet
    {
        public XmlSchemaWhiteSpaceFacet()
        {
            base.FacetType = FacetType.Whitespace;
        }
    }
}

