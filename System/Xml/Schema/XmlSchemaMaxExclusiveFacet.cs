namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMaxExclusiveFacet : XmlSchemaFacet
    {
        public XmlSchemaMaxExclusiveFacet()
        {
            base.FacetType = FacetType.MaxExclusive;
        }
    }
}

