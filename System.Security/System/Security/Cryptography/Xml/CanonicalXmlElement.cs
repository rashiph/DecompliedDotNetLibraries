namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class CanonicalXmlElement : XmlElement, ICanonicalizableNode
    {
        private bool m_isInNodeSet;

        public CanonicalXmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc, bool defaultNodeSetInclusionState) : base(prefix, localName, namespaceURI, doc)
        {
            this.m_isInNodeSet = defaultNodeSetInclusionState;
        }

        public void Write(StringBuilder strBuilder, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            Hashtable nsLocallyDeclared = new Hashtable();
            SortedList nsListToRender = new SortedList(new NamespaceSortOrder());
            SortedList attrListToRender = new SortedList(new AttributeSortOrder());
            XmlAttributeCollection attributes = this.Attributes;
            if (attributes != null)
            {
                foreach (XmlAttribute attribute in attributes)
                {
                    if ((((CanonicalXmlAttribute) attribute).IsInNodeSet || System.Security.Cryptography.Xml.Utils.IsNamespaceNode(attribute)) || System.Security.Cryptography.Xml.Utils.IsXmlNamespaceNode(attribute))
                    {
                        if (System.Security.Cryptography.Xml.Utils.IsNamespaceNode(attribute))
                        {
                            anc.TrackNamespaceNode(attribute, nsListToRender, nsLocallyDeclared);
                        }
                        else if (System.Security.Cryptography.Xml.Utils.IsXmlNamespaceNode(attribute))
                        {
                            anc.TrackXmlNamespaceNode(attribute, nsListToRender, attrListToRender, nsLocallyDeclared);
                        }
                        else if (this.IsInNodeSet)
                        {
                            attrListToRender.Add(attribute, null);
                        }
                    }
                }
            }
            if (!System.Security.Cryptography.Xml.Utils.IsCommittedNamespace(this, this.Prefix, this.NamespaceURI))
            {
                string name = (this.Prefix.Length > 0) ? ("xmlns:" + this.Prefix) : "xmlns";
                XmlAttribute attr = this.OwnerDocument.CreateAttribute(name);
                attr.Value = this.NamespaceURI;
                anc.TrackNamespaceNode(attr, nsListToRender, nsLocallyDeclared);
            }
            if (this.IsInNodeSet)
            {
                anc.GetNamespacesToRender(this, attrListToRender, nsListToRender, nsLocallyDeclared);
                strBuilder.Append("<" + this.Name);
                foreach (object obj2 in nsListToRender.GetKeyList())
                {
                    (obj2 as CanonicalXmlAttribute).Write(strBuilder, docPos, anc);
                }
                foreach (object obj3 in attrListToRender.GetKeyList())
                {
                    (obj3 as CanonicalXmlAttribute).Write(strBuilder, docPos, anc);
                }
                strBuilder.Append(">");
            }
            anc.EnterElementContext();
            anc.LoadUnrenderedNamespaces(nsLocallyDeclared);
            anc.LoadRenderedNamespaces(nsListToRender);
            foreach (XmlNode node in this.ChildNodes)
            {
                CanonicalizationDispatcher.Write(node, strBuilder, docPos, anc);
            }
            anc.ExitElementContext();
            if (this.IsInNodeSet)
            {
                strBuilder.Append("</" + this.Name + ">");
            }
        }

        public void WriteHash(HashAlgorithm hash, DocPosition docPos, AncestralNamespaceContextManager anc)
        {
            byte[] bytes;
            Hashtable nsLocallyDeclared = new Hashtable();
            SortedList nsListToRender = new SortedList(new NamespaceSortOrder());
            SortedList attrListToRender = new SortedList(new AttributeSortOrder());
            UTF8Encoding encoding = new UTF8Encoding(false);
            XmlAttributeCollection attributes = this.Attributes;
            if (attributes != null)
            {
                foreach (XmlAttribute attribute in attributes)
                {
                    if ((((CanonicalXmlAttribute) attribute).IsInNodeSet || System.Security.Cryptography.Xml.Utils.IsNamespaceNode(attribute)) || System.Security.Cryptography.Xml.Utils.IsXmlNamespaceNode(attribute))
                    {
                        if (System.Security.Cryptography.Xml.Utils.IsNamespaceNode(attribute))
                        {
                            anc.TrackNamespaceNode(attribute, nsListToRender, nsLocallyDeclared);
                        }
                        else if (System.Security.Cryptography.Xml.Utils.IsXmlNamespaceNode(attribute))
                        {
                            anc.TrackXmlNamespaceNode(attribute, nsListToRender, attrListToRender, nsLocallyDeclared);
                        }
                        else if (this.IsInNodeSet)
                        {
                            attrListToRender.Add(attribute, null);
                        }
                    }
                }
            }
            if (!System.Security.Cryptography.Xml.Utils.IsCommittedNamespace(this, this.Prefix, this.NamespaceURI))
            {
                string name = (this.Prefix.Length > 0) ? ("xmlns:" + this.Prefix) : "xmlns";
                XmlAttribute attr = this.OwnerDocument.CreateAttribute(name);
                attr.Value = this.NamespaceURI;
                anc.TrackNamespaceNode(attr, nsListToRender, nsLocallyDeclared);
            }
            if (this.IsInNodeSet)
            {
                anc.GetNamespacesToRender(this, attrListToRender, nsListToRender, nsLocallyDeclared);
                bytes = encoding.GetBytes("<" + this.Name);
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
                foreach (object obj2 in nsListToRender.GetKeyList())
                {
                    (obj2 as CanonicalXmlAttribute).WriteHash(hash, docPos, anc);
                }
                foreach (object obj3 in attrListToRender.GetKeyList())
                {
                    (obj3 as CanonicalXmlAttribute).WriteHash(hash, docPos, anc);
                }
                bytes = encoding.GetBytes(">");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
            anc.EnterElementContext();
            anc.LoadUnrenderedNamespaces(nsLocallyDeclared);
            anc.LoadRenderedNamespaces(nsListToRender);
            foreach (XmlNode node in this.ChildNodes)
            {
                CanonicalizationDispatcher.WriteHash(node, hash, docPos, anc);
            }
            anc.ExitElementContext();
            if (this.IsInNodeSet)
            {
                bytes = encoding.GetBytes("</" + this.Name + ">");
                hash.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
            }
        }

        public bool IsInNodeSet
        {
            get
            {
                return this.m_isInNodeSet;
            }
            set
            {
                this.m_isInNodeSet = value;
            }
        }
    }
}

