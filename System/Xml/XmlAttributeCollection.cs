namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime;

    public sealed class XmlAttributeCollection : XmlNamedNodeMap, ICollection, IEnumerable
    {
        internal XmlAttributeCollection(XmlNode parent) : base(parent)
        {
        }

        internal override XmlNode AddNode(XmlNode node)
        {
            this.RemoveDuplicateAttribute((XmlAttribute) node);
            XmlNode node2 = base.AddNode(node);
            this.InsertParentIntoElementIdAttrMap((XmlAttribute) node);
            return node2;
        }

        public XmlAttribute Append(XmlAttribute node)
        {
            XmlDocument ownerDocument = node.OwnerDocument;
            if ((ownerDocument == null) || !ownerDocument.IsLoading)
            {
                if ((ownerDocument != null) && (ownerDocument != base.parent.OwnerDocument))
                {
                    throw new ArgumentException(Res.GetString("Xdom_NamedNode_Context"));
                }
                if (node.OwnerElement != null)
                {
                    this.Detach(node);
                }
                this.AddNode(node);
                return node;
            }
            base.AddNodeForLoad(node, ownerDocument);
            this.InsertParentIntoElementIdAttrMap(node);
            return node;
        }

        public void CopyTo(XmlAttribute[] array, int index)
        {
            int num = 0;
            int count = this.Count;
            while (num < count)
            {
                array[index] = (XmlAttribute) ((XmlNode) base.nodes[num]).CloneNode(true);
                num++;
                index++;
            }
        }

        internal void Detach(XmlAttribute attr)
        {
            attr.OwnerElement.Attributes.Remove(attr);
        }

        internal int FindNodeOffset(XmlAttribute node)
        {
            ArrayList nodes = base.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlAttribute attribute = (XmlAttribute) nodes[i];
                if (((attribute.LocalNameHash == node.LocalNameHash) && (attribute.Name == node.Name)) && (attribute.NamespaceURI == node.NamespaceURI))
                {
                    return i;
                }
            }
            return -1;
        }

        internal int FindNodeOffsetNS(XmlAttribute node)
        {
            ArrayList nodes = base.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlAttribute attribute = (XmlAttribute) nodes[i];
                if (((attribute.LocalNameHash == node.LocalNameHash) && (attribute.LocalName == node.LocalName)) && (attribute.NamespaceURI == node.NamespaceURI))
                {
                    return i;
                }
            }
            return -1;
        }

        public XmlAttribute InsertAfter(XmlAttribute newNode, XmlAttribute refNode)
        {
            if (newNode != refNode)
            {
                if (refNode == null)
                {
                    return this.Prepend(newNode);
                }
                if (refNode.OwnerElement != base.parent)
                {
                    throw new ArgumentException(Res.GetString("Xdom_AttrCol_Insert"));
                }
                if ((newNode.OwnerDocument != null) && (newNode.OwnerDocument != base.parent.OwnerDocument))
                {
                    throw new ArgumentException(Res.GetString("Xdom_NamedNode_Context"));
                }
                if (newNode.OwnerElement != null)
                {
                    this.Detach(newNode);
                }
                int num = base.FindNodeOffset(refNode.LocalName, refNode.NamespaceURI);
                int num2 = this.RemoveDuplicateAttribute(newNode);
                if ((num2 >= 0) && (num2 < num))
                {
                    num--;
                }
                this.InsertNodeAt(num + 1, newNode);
            }
            return newNode;
        }

        public XmlAttribute InsertBefore(XmlAttribute newNode, XmlAttribute refNode)
        {
            if (newNode != refNode)
            {
                if (refNode == null)
                {
                    return this.Append(newNode);
                }
                if (refNode.OwnerElement != base.parent)
                {
                    throw new ArgumentException(Res.GetString("Xdom_AttrCol_Insert"));
                }
                if ((newNode.OwnerDocument != null) && (newNode.OwnerDocument != base.parent.OwnerDocument))
                {
                    throw new ArgumentException(Res.GetString("Xdom_NamedNode_Context"));
                }
                if (newNode.OwnerElement != null)
                {
                    this.Detach(newNode);
                }
                int i = base.FindNodeOffset(refNode.LocalName, refNode.NamespaceURI);
                int num2 = this.RemoveDuplicateAttribute(newNode);
                if ((num2 >= 0) && (num2 < i))
                {
                    i--;
                }
                this.InsertNodeAt(i, newNode);
            }
            return newNode;
        }

        internal override XmlNode InsertNodeAt(int i, XmlNode node)
        {
            XmlNode node2 = base.InsertNodeAt(i, node);
            this.InsertParentIntoElementIdAttrMap((XmlAttribute) node);
            return node2;
        }

        internal void InsertParentIntoElementIdAttrMap(XmlAttribute attr)
        {
            XmlElement parent = base.parent as XmlElement;
            if ((parent != null) && (base.parent.OwnerDocument != null))
            {
                XmlName iDInfoByElement = base.parent.OwnerDocument.GetIDInfoByElement(parent.XmlName);
                if (((iDInfoByElement != null) && (iDInfoByElement.Prefix == attr.XmlName.Prefix)) && (iDInfoByElement.LocalName == attr.XmlName.LocalName))
                {
                    base.parent.OwnerDocument.AddElementWithId(attr.Value, parent);
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal XmlAttribute InternalAppendAttribute(XmlAttribute node)
        {
            XmlNode node2 = base.AddNode(node);
            this.InsertParentIntoElementIdAttrMap(node);
            return (XmlAttribute) node2;
        }

        internal bool PrepareParentInElementIdAttrMap(string attrPrefix, string attrLocalName)
        {
            XmlElement parent = base.parent as XmlElement;
            XmlName iDInfoByElement = base.parent.OwnerDocument.GetIDInfoByElement(parent.XmlName);
            return (((iDInfoByElement != null) && (iDInfoByElement.Prefix == attrPrefix)) && (iDInfoByElement.LocalName == attrLocalName));
        }

        public XmlAttribute Prepend(XmlAttribute node)
        {
            if ((node.OwnerDocument != null) && (node.OwnerDocument != base.parent.OwnerDocument))
            {
                throw new ArgumentException(Res.GetString("Xdom_NamedNode_Context"));
            }
            if (node.OwnerElement != null)
            {
                this.Detach(node);
            }
            this.RemoveDuplicateAttribute(node);
            this.InsertNodeAt(0, node);
            return node;
        }

        public XmlAttribute Remove(XmlAttribute node)
        {
            if (base.nodes != null)
            {
                int count = base.nodes.Count;
                for (int i = 0; i < count; i++)
                {
                    if (base.nodes[i] == node)
                    {
                        this.RemoveNodeAt(i);
                        return node;
                    }
                }
            }
            return null;
        }

        public void RemoveAll()
        {
            int count = this.Count;
            while (count > 0)
            {
                count--;
                this.RemoveAt(count);
            }
        }

        public XmlAttribute RemoveAt(int i)
        {
            if (((i >= 0) && (i < this.Count)) && (base.nodes != null))
            {
                return (XmlAttribute) this.RemoveNodeAt(i);
            }
            return null;
        }

        internal int RemoveDuplicateAttribute(XmlAttribute attr)
        {
            int i = base.FindNodeOffset(attr.LocalName, attr.NamespaceURI);
            if (i != -1)
            {
                XmlAttribute attribute = (XmlAttribute) base.Nodes[i];
                base.RemoveNodeAt(i);
                this.RemoveParentFromElementIdAttrMap(attribute);
            }
            return i;
        }

        internal override XmlNode RemoveNodeAt(int i)
        {
            XmlNode node = base.RemoveNodeAt(i);
            this.RemoveParentFromElementIdAttrMap((XmlAttribute) node);
            XmlAttribute attribute = base.parent.OwnerDocument.GetDefaultAttribute((XmlElement) base.parent, node.Prefix, node.LocalName, node.NamespaceURI);
            if (attribute != null)
            {
                this.InsertNodeAt(i, attribute);
            }
            return node;
        }

        internal void RemoveParentFromElementIdAttrMap(XmlAttribute attr)
        {
            XmlElement parent = base.parent as XmlElement;
            if ((parent != null) && (base.parent.OwnerDocument != null))
            {
                XmlName iDInfoByElement = base.parent.OwnerDocument.GetIDInfoByElement(parent.XmlName);
                if (((iDInfoByElement != null) && (iDInfoByElement.Prefix == attr.XmlName.Prefix)) && (iDInfoByElement.LocalName == attr.XmlName.LocalName))
                {
                    base.parent.OwnerDocument.RemoveElementWithId(attr.Value, parent);
                }
            }
        }

        internal void ResetParentInElementIdAttrMap(string oldVal, string newVal)
        {
            XmlElement parent = base.parent as XmlElement;
            XmlDocument ownerDocument = base.parent.OwnerDocument;
            ownerDocument.RemoveElementWithId(oldVal, parent);
            ownerDocument.AddElementWithId(newVal, parent);
        }

        public override XmlNode SetNamedItem(XmlNode node)
        {
            if ((node != null) && !(node is XmlAttribute))
            {
                throw new ArgumentException(Res.GetString("Xdom_AttrCol_Object"));
            }
            int i = base.FindNodeOffset(node.LocalName, node.NamespaceURI);
            if (i == -1)
            {
                return this.InternalAppendAttribute((XmlAttribute) node);
            }
            XmlNode node2 = base.RemoveNodeAt(i);
            this.InsertNodeAt(i, node);
            return node2;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            int num = 0;
            int count = base.Nodes.Count;
            while (num < count)
            {
                array.SetValue(base.nodes[num], index);
                num++;
                index++;
            }
        }

        public XmlAttribute this[int i]
        {
            get
            {
                XmlAttribute attribute;
                try
                {
                    attribute = (XmlAttribute) base.Nodes[i];
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new IndexOutOfRangeException(Res.GetString("Xdom_IndexOutOfRange"));
                }
                return attribute;
            }
        }

        public XmlAttribute this[string name]
        {
            get
            {
                ArrayList nodes = base.Nodes;
                int hashCode = XmlName.GetHashCode(name);
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlAttribute attribute = (XmlAttribute) nodes[i];
                    if ((hashCode == attribute.LocalNameHash) && (name == attribute.Name))
                    {
                        return attribute;
                    }
                }
                return null;
            }
        }

        public XmlAttribute this[string localName, string namespaceURI]
        {
            get
            {
                ArrayList nodes = base.Nodes;
                int hashCode = XmlName.GetHashCode(localName);
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlAttribute attribute = (XmlAttribute) nodes[i];
                    if (((hashCode == attribute.LocalNameHash) && (localName == attribute.LocalName)) && (namespaceURI == attribute.NamespaceURI))
                    {
                        return attribute;
                    }
                }
                return null;
            }
        }

        int ICollection.Count
        {
            get
            {
                return base.Count;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

