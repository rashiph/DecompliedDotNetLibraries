namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalizationDispatcher
    {
        private CanonicalizationDispatcher()
        {
        }

        public static void Write(XmlNode node, StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (node is ICanonicalizableNode)
            {
                ((ICanonicalizableNode) node).Write(strBuilder, docPos, anc);
            }
            else
            {
                WriteGenericNode(node, strBuilder, docPos, anc);
            }
        }

        public static void WriteGenericNode(XmlNode node, StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            foreach (XmlNode node2 in node.ChildNodes)
            {
                Write(node2, strBuilder, docPos, anc);
            }
        }

        public static void WriteHash(XmlNode node, HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (node is ICanonicalizableNode)
            {
                ((ICanonicalizableNode) node).WriteHash(hash, docPos, anc);
            }
            else
            {
                WriteHashGenericNode(node, hash, docPos, anc);
            }
        }

        public static void WriteHashGenericNode(XmlNode node, HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            foreach (XmlNode node2 in node.ChildNodes)
            {
                WriteHash(node2, hash, docPos, anc);
            }
        }
    }
}

