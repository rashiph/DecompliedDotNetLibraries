namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Xml.Schema;

    internal class XmlFacetComparer : IComparer
    {
        public int Compare(object o1, object o2)
        {
            XmlSchemaFacet facet = (XmlSchemaFacet) o1;
            XmlSchemaFacet facet2 = (XmlSchemaFacet) o2;
            return string.Compare(facet.GetType().Name + ":" + facet.Value, facet2.GetType().Name + ":" + facet2.Value, StringComparison.Ordinal);
        }
    }
}

