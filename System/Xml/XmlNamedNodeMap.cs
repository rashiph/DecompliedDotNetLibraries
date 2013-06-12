namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Runtime;

    public class XmlNamedNodeMap : IEnumerable
    {
        internal ArrayList nodes;
        internal XmlNode parent;

        internal XmlNamedNodeMap(XmlNode parent)
        {
            this.parent = parent;
            this.nodes = null;
        }

        internal virtual XmlNode AddNode(XmlNode node)
        {
            XmlNode ownerElement;
            if (node.NodeType == XmlNodeType.Attribute)
            {
                ownerElement = ((XmlAttribute) node).OwnerElement;
            }
            else
            {
                ownerElement = node.ParentNode;
            }
            string oldValue = node.Value;
            XmlNodeChangedEventArgs args = this.parent.GetEventArgs(node, ownerElement, this.parent, oldValue, oldValue, XmlNodeChangedAction.Insert);
            if (args != null)
            {
                this.parent.BeforeEvent(args);
            }
            this.Nodes.Add(node);
            node.SetParent(this.parent);
            if (args != null)
            {
                this.parent.AfterEvent(args);
            }
            return node;
        }

        internal virtual XmlNode AddNodeForLoad(XmlNode node, XmlDocument doc)
        {
            XmlNodeChangedEventArgs insertEventArgsForLoad = doc.GetInsertEventArgsForLoad(node, this.parent);
            if (insertEventArgsForLoad != null)
            {
                doc.BeforeEvent(insertEventArgsForLoad);
            }
            this.Nodes.Add(node);
            node.SetParent(this.parent);
            if (insertEventArgsForLoad != null)
            {
                doc.AfterEvent(insertEventArgsForLoad);
            }
            return node;
        }

        internal int FindNodeOffset(string name)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                XmlNode node = (XmlNode) this.Nodes[i];
                if (name == node.Name)
                {
                    return i;
                }
            }
            return -1;
        }

        internal int FindNodeOffset(string localName, string namespaceURI)
        {
            int count = this.Count;
            for (int i = 0; i < count; i++)
            {
                XmlNode node = (XmlNode) this.Nodes[i];
                if ((node.LocalName == localName) && (node.NamespaceURI == namespaceURI))
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual IEnumerator GetEnumerator()
        {
            if (this.nodes == null)
            {
                return XmlDocument.EmptyEnumerator;
            }
            return this.Nodes.GetEnumerator();
        }

        public virtual XmlNode GetNamedItem(string name)
        {
            int num = this.FindNodeOffset(name);
            if (num >= 0)
            {
                return (XmlNode) this.Nodes[num];
            }
            return null;
        }

        public virtual XmlNode GetNamedItem(string localName, string namespaceURI)
        {
            int num = this.FindNodeOffset(localName, namespaceURI);
            if (num >= 0)
            {
                return (XmlNode) this.Nodes[num];
            }
            return null;
        }

        internal virtual XmlNode InsertNodeAt(int i, XmlNode node)
        {
            XmlNode ownerElement;
            if (node.NodeType == XmlNodeType.Attribute)
            {
                ownerElement = ((XmlAttribute) node).OwnerElement;
            }
            else
            {
                ownerElement = node.ParentNode;
            }
            string oldValue = node.Value;
            XmlNodeChangedEventArgs args = this.parent.GetEventArgs(node, ownerElement, this.parent, oldValue, oldValue, XmlNodeChangedAction.Insert);
            if (args != null)
            {
                this.parent.BeforeEvent(args);
            }
            this.Nodes.Insert(i, node);
            node.SetParent(this.parent);
            if (args != null)
            {
                this.parent.AfterEvent(args);
            }
            return node;
        }

        public virtual XmlNode Item(int index)
        {
            XmlNode node;
            if ((index < 0) || (index >= this.Nodes.Count))
            {
                return null;
            }
            try
            {
                node = (XmlNode) this.Nodes[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new IndexOutOfRangeException(Res.GetString("Xdom_IndexOutOfRange"));
            }
            return node;
        }

        public virtual XmlNode RemoveNamedItem(string name)
        {
            int i = this.FindNodeOffset(name);
            if (i >= 0)
            {
                return this.RemoveNodeAt(i);
            }
            return null;
        }

        public virtual XmlNode RemoveNamedItem(string localName, string namespaceURI)
        {
            int i = this.FindNodeOffset(localName, namespaceURI);
            if (i >= 0)
            {
                return this.RemoveNodeAt(i);
            }
            return null;
        }

        internal virtual XmlNode RemoveNodeAt(int i)
        {
            XmlNode node = (XmlNode) this.Nodes[i];
            string oldValue = node.Value;
            XmlNodeChangedEventArgs args = this.parent.GetEventArgs(node, this.parent, null, oldValue, oldValue, XmlNodeChangedAction.Remove);
            if (args != null)
            {
                this.parent.BeforeEvent(args);
            }
            this.Nodes.RemoveAt(i);
            node.SetParent(null);
            if (args != null)
            {
                this.parent.AfterEvent(args);
            }
            return node;
        }

        internal XmlNode ReplaceNodeAt(int i, XmlNode node)
        {
            XmlNode node2 = this.RemoveNodeAt(i);
            this.InsertNodeAt(i, node);
            return node2;
        }

        public virtual XmlNode SetNamedItem(XmlNode node)
        {
            if (node == null)
            {
                return null;
            }
            int i = this.FindNodeOffset(node.LocalName, node.NamespaceURI);
            if (i == -1)
            {
                this.AddNode(node);
                return null;
            }
            return this.ReplaceNodeAt(i, node);
        }

        public virtual int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                if (this.nodes != null)
                {
                    return this.nodes.Count;
                }
                return 0;
            }
        }

        internal ArrayList Nodes
        {
            get
            {
                if (this.nodes == null)
                {
                    this.nodes = new ArrayList();
                }
                return this.nodes;
            }
        }
    }
}

