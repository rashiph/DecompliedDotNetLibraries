namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXml
    {
        private C14NAncestralNamespaceContextManager m_ancMgr;
        private CanonicalXmlDocument m_c14nDoc;

        internal CanonicalXml(XmlDocument document, XmlResolver resolver) : this(document, resolver, false)
        {
        }

        internal CanonicalXml(XmlDocument document, XmlResolver resolver, bool includeComments)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            this.m_c14nDoc = new CanonicalXmlDocument(true, includeComments);
            this.m_c14nDoc.XmlResolver = resolver;
            this.m_c14nDoc.Load(new XmlNodeReader(document));
            this.m_ancMgr = new C14NAncestralNamespaceContextManager();
        }

        internal CanonicalXml(XmlNodeList nodeList, XmlResolver resolver, bool includeComments)
        {
            if (nodeList == null)
            {
                throw new ArgumentNullException("nodeList");
            }
            XmlDocument ownerDocument = System.Security.Cryptography.Xml.Utils.GetOwnerDocument(nodeList);
            if (ownerDocument == null)
            {
                throw new ArgumentException("nodeList");
            }
            this.m_c14nDoc = new CanonicalXmlDocument(false, includeComments);
            this.m_c14nDoc.XmlResolver = resolver;
            this.m_c14nDoc.Load(new XmlNodeReader(ownerDocument));
            this.m_ancMgr = new C14NAncestralNamespaceContextManager();
            MarkInclusionStateForNodes(nodeList, ownerDocument, this.m_c14nDoc);
        }

        internal CanonicalXml(Stream inputStream, bool includeComments, XmlResolver resolver, string strBaseUri)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }
            this.m_c14nDoc = new CanonicalXmlDocument(true, includeComments);
            this.m_c14nDoc.XmlResolver = resolver;
            this.m_c14nDoc.Load(System.Security.Cryptography.Xml.Utils.PreProcessStreamInput(inputStream, resolver, strBaseUri));
            this.m_ancMgr = new C14NAncestralNamespaceContextManager();
        }

        internal byte[] GetBytes()
        {
            StringBuilder strBuilder = new StringBuilder();
            this.m_c14nDoc.Write(strBuilder, DocPosition.BeforeRootElement, this.m_ancMgr);
            UTF8Encoding encoding = new UTF8Encoding(false);
            return encoding.GetBytes(strBuilder.ToString());
        }

        internal byte[] GetDigestedBytes(HashAlgorithm hash)
        {
            this.m_c14nDoc.WriteHash(hash, DocPosition.BeforeRootElement, this.m_ancMgr);
            hash.TransformFinalBlock(new byte[0], 0, 0);
            byte[] buffer = (byte[]) hash.Hash.Clone();
            hash.Initialize();
            return buffer;
        }

        private static void MarkInclusionStateForNodes(XmlNodeList nodeList, XmlDocument inputRoot, XmlDocument root)
        {
            CanonicalXmlNodeList list = new CanonicalXmlNodeList();
            CanonicalXmlNodeList list2 = new CanonicalXmlNodeList();
            list.Add(inputRoot);
            list2.Add(root);
            int num = 0;
            do
            {
                XmlNode node = list[num];
                XmlNode node2 = list2[num];
                XmlNodeList childNodes = node.ChildNodes;
                XmlNodeList list4 = node2.ChildNodes;
                for (int i = 0; i < childNodes.Count; i++)
                {
                    list.Add(childNodes[i]);
                    list2.Add(list4[i]);
                    if (System.Security.Cryptography.Xml.Utils.NodeInList(childNodes[i], nodeList))
                    {
                        MarkNodeAsIncluded(list4[i]);
                    }
                    XmlAttributeCollection attributes = childNodes[i].Attributes;
                    if (attributes != null)
                    {
                        for (int j = 0; j < attributes.Count; j++)
                        {
                            if (System.Security.Cryptography.Xml.Utils.NodeInList(attributes[j], nodeList))
                            {
                                MarkNodeAsIncluded(list4[i].Attributes.Item(j));
                            }
                        }
                    }
                }
                num++;
            }
            while (num < list.Count);
        }

        private static void MarkNodeAsIncluded(XmlNode node)
        {
            if (node is ICanonicalizableNode)
            {
                ((ICanonicalizableNode) node).IsInNodeSet = true;
            }
        }
    }
}

