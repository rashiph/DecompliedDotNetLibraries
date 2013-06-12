namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMaxInclusiveFacet : XmlSchemaFacet
    {
        public XmlSchemaMaxInclusiveFacet()
        {
            base.FacetType = FacetType.MaxInclusive;
        }
    }
}

