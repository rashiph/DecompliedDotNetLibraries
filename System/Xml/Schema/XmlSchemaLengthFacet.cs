namespace System.Xml.Schema
{
    using System;

    public class XmlSchemaLengthFacet : XmlSchemaNumericFacet
    {
        public XmlSchemaLengthFacet()
        {
            base.FacetType = FacetType.Length;
        }
    }
}

