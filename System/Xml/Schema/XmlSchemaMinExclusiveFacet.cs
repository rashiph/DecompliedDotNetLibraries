namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaMinExclusiveFacet : XmlSchemaFacet
    {
        public XmlSchemaMinExclusiveFacet()
        {
            base.FacetType = FacetType.MinExclusive;
        }
    }
}

