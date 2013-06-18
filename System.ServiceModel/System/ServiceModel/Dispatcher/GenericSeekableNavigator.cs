namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Reflection;
    using System.Xml;
    using System.Xml.XPath;

    internal class GenericSeekableNavigator : SeekableXPathNavigator
    {
        private long currentPosition;
        private GenericSeekableNavigator dom;
        private XPathNavigator navigator;
        private QueryBuffer<XPathNavigator> nodes;

        internal GenericSeekableNavigator(GenericSeekableNavigator navigator)
        {
            this.navigator = navigator.navigator.Clone();
            this.nodes = new QueryBuffer<XPathNavigator>();
            this.currentPosition = navigator.currentPosition;
            this.dom = navigator.dom;
        }

        internal GenericSeekableNavigator(XPathNavigator navigator)
        {
            this.navigator = navigator;
            this.nodes = new QueryBuffer<XPathNavigator>(4);
            this.currentPosition = -1L;
            this.dom = this;
        }

        public override XPathNavigator Clone()
        {
            return new GenericSeekableNavigator(this);
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator navigator)
        {
            if (navigator != null)
            {
                GenericSeekableNavigator navigator2 = navigator as GenericSeekableNavigator;
                if (navigator2 != null)
                {
                    return this.navigator.ComparePosition(navigator2.navigator);
                }
            }
            return XmlNodeOrder.Unknown;
        }

        public override XmlNodeOrder ComparePosition(long x, long y)
        {
            XPathNavigator navigator = this[x];
            XPathNavigator nav = this[y];
            return navigator.ComparePosition(nav);
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            return this.navigator.GetAttribute(localName, namespaceURI);
        }

        public override string GetLocalName(long nodePosition)
        {
            return this[nodePosition].LocalName;
        }

        public override string GetName(long nodePosition)
        {
            return this[nodePosition].Name;
        }

        public override string GetNamespace(long nodePosition)
        {
            return this[nodePosition].NamespaceURI;
        }

        public override string GetNamespace(string name)
        {
            return this.navigator.GetNamespace(name);
        }

        public override XPathNodeType GetNodeType(long nodePosition)
        {
            return this[nodePosition].NodeType;
        }

        public override string GetValue(long nodePosition)
        {
            return this[nodePosition].Value;
        }

        public override bool IsDescendant(XPathNavigator navigator)
        {
            if (navigator == null)
            {
                return false;
            }
            GenericSeekableNavigator navigator2 = navigator as GenericSeekableNavigator;
            return ((navigator2 != null) && this.navigator.IsDescendant(navigator2.navigator));
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            GenericSeekableNavigator navigator = other as GenericSeekableNavigator;
            return ((navigator != null) && this.navigator.IsSamePosition(navigator.navigator));
        }

        public override bool MoveTo(XPathNavigator other)
        {
            GenericSeekableNavigator navigator = other as GenericSeekableNavigator;
            if ((navigator != null) && this.navigator.MoveTo(navigator.navigator))
            {
                this.currentPosition = navigator.currentPosition;
                return true;
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToAttribute(localName, namespaceURI);
        }

        public override bool MoveToFirst()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToFirst();
        }

        public override bool MoveToFirstAttribute()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToFirstAttribute();
        }

        public override bool MoveToFirstChild()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToId(string id)
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToId(id);
        }

        public override bool MoveToNamespace(string name)
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToNamespace(name);
        }

        public override bool MoveToNext()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToNext();
        }

        public override bool MoveToNextAttribute()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToNextAttribute();
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToParent()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToParent();
        }

        public override bool MoveToPrevious()
        {
            this.currentPosition = -1L;
            return this.navigator.MoveToPrevious();
        }

        public override void MoveToRoot()
        {
            this.currentPosition = -1L;
            this.navigator.MoveToRoot();
        }

        internal void SnapshotNavigator()
        {
            this.currentPosition = this.dom.nodes.Count;
            this.dom.nodes.Add(this.navigator.Clone());
        }

        public override string BaseURI
        {
            get
            {
                return this.navigator.BaseURI;
            }
        }

        public override long CurrentPosition
        {
            get
            {
                if (-1L == this.currentPosition)
                {
                    this.SnapshotNavigator();
                }
                return this.currentPosition;
            }
            set
            {
                this.navigator.MoveTo(this[value]);
                this.currentPosition = value;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return this.navigator.HasAttributes;
            }
        }

        public override bool HasChildren
        {
            get
            {
                return this.navigator.HasChildren;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this.navigator.IsEmptyElement;
            }
        }

        internal XPathNavigator this[long nodePosition]
        {
            get
            {
                int num = (int) nodePosition;
                return this.dom.nodes[num];
            }
        }

        public override string LocalName
        {
            get
            {
                return this.navigator.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this.navigator.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this.navigator.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this.navigator.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.navigator.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this.navigator.Prefix;
            }
        }

        public override string Value
        {
            get
            {
                return this.navigator.Value;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this.navigator.XmlLang;
            }
        }
    }
}

