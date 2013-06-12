namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaEnumerationFacet : XmlSchemaFacet
    {
        public XmlSchemaEnumerationFacet()
        {
            base.FacetType = FacetType.Enumeration;
        }
    }
}

