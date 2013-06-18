namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Xml;

    internal class NamespaceSortOrder : IComparer
    {
        internal NamespaceSortOrder()
        {
        }

        public int Compare(object a, object b)
        {
            XmlNode n = a as XmlNode;
            XmlNode node2 = b as XmlNode;
            if ((a == null) || (b == null))
            {
                throw new ArgumentException();
            }
            bool flag = Utils.IsDefaultNamespaceNode(n);
            bool flag2 = Utils.IsDefaultNamespaceNode(node2);
            if (flag && flag2)
            {
                return 0;
            }
            if (flag)
            {
                return -1;
            }
            if (flag2)
            {
                return 1;
            }
            return string.CompareOrdinal(n.LocalName, node2.LocalName);
        }
    }
}

