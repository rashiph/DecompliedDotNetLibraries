namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class AttributeSortOrder : IComparer
    {
        internal AttributeSortOrder()
        {
        }

        public int Compare(object a, object b)
        {
            XmlNode node = a as XmlNode;
            XmlNode node2 = b as XmlNode;
            if ((a == null) || (b == null))
            {
                throw new ArgumentException();
            }
            int num = string.CompareOrdinal(node.NamespaceURI, node2.NamespaceURI);
            if (num != 0)
            {
                return num;
            }
            return string.CompareOrdinal(node.LocalName, node2.LocalName);
        }
    }
}

