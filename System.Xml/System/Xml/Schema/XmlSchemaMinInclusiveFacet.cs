namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMinInclusiveFacet : XmlSchemaFacet
    {
        public XmlSchemaMinInclusiveFacet()
        {
            base.FacetType = FacetType.MinInclusive;
        }
    }
}

