namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal sealed class DataDocumentXPathNavigator : XPathNavigator, IHasXmlNode
    {
        private XPathNodePointer _curNode;
        private XmlDataDocument _doc;
        private XPathNodePointer _temp;

        private DataDocumentXPathNavigator(DataDocumentXPathNavigator other)
        {
            this._curNode = other._curNode.Clone(this);
            this._temp = other._temp.Clone(this);
            this._doc = other._doc;
        }

        internal DataDocumentXPathNavigator(XmlDataDocument doc, XmlNode node)
        {
            this._curNode = new XPathNodePointer(this, doc, node);
            this._temp = new XPathNodePointer(this, doc, node);
            this._doc = doc;
        }

        public override XPathNavigator Clone()
        {
            return new DataDocumentXPathNavigator(this);
        }

        public override XmlNodeOrder ComparePosition(XPathNavigator other)
        {
            if (other != null)
            {
                DataDocumentXPathNavigator navigator = other as DataDocumentXPathNavigator;
                if ((navigator != null) && (navigator.Document == this._doc))
                {
                    return this._curNode.ComparePosition(navigator.CurNode);
                }
            }
            return XmlNodeOrder.Unknown;
        }

        public override string GetAttribute(string localName, string namespaceURI)
        {
            if (this._curNode.NodeType == XPathNodeType.Element)
            {
                this._temp.MoveTo(this._curNode);
                if (this._temp.MoveToAttribute(localName, namespaceURI))
                {
                    return this._temp.Value;
                }
            }
            return string.Empty;
        }

        public override string GetNamespace(string name)
        {
            return this._curNode.GetNamespace(name);
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            if (other == null)
            {
                return false;
            }
            DataDocumentXPathNavigator navigator = other as DataDocumentXPathNavigator;
            return (((navigator != null) && (this._doc == navigator.Document)) && this._curNode.IsSamePosition(navigator.CurNode));
        }

        public override bool MoveTo(XPathNavigator other)
        {
            if (other != null)
            {
                DataDocumentXPathNavigator navigator = other as DataDocumentXPathNavigator;
                if (navigator == null)
                {
                    return false;
                }
                if (this._curNode.MoveTo(navigator.CurNode))
                {
                    this._doc = this._curNode.Document;
                    return true;
                }
            }
            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (this._curNode.NodeType != XPathNodeType.Element)
            {
                return false;
            }
            return this._curNode.MoveToAttribute(localName, namespaceURI);
        }

        public override bool MoveToFirst()
        {
            if (this._curNode.NodeType == XPathNodeType.Attribute)
            {
                return false;
            }
            return this._curNode.MoveToFirst();
        }

        public override bool MoveToFirstAttribute()
        {
            if (this._curNode.NodeType != XPathNodeType.Element)
            {
                return false;
            }
            return this._curNode.MoveToNextAttribute(true);
        }

        public override bool MoveToFirstChild()
        {
            return this._curNode.MoveToFirstChild();
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            if (this._curNode.NodeType != XPathNodeType.Element)
            {
                return false;
            }
            return this._curNode.MoveToFirstNamespace(namespaceScope);
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNamespace(string name)
        {
            if (this._curNode.NodeType != XPathNodeType.Element)
            {
                return false;
            }
            return this._curNode.MoveToNamespace(name);
        }

        public override bool MoveToNext()
        {
            if (this._curNode.NodeType == XPathNodeType.Attribute)
            {
                return false;
            }
            return this._curNode.MoveToNextSibling();
        }

        public override bool MoveToNextAttribute()
        {
            if (this._curNode.NodeType != XPathNodeType.Attribute)
            {
                return false;
            }
            return this._curNode.MoveToNextAttribute(false);
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            if (this._curNode.NodeType != XPathNodeType.Namespace)
            {
                return false;
            }
            return this._curNode.MoveToNextNamespace(namespaceScope);
        }

        public override bool MoveToParent()
        {
            return this._curNode.MoveToParent();
        }

        public override bool MoveToPrevious()
        {
            if (this._curNode.NodeType == XPathNodeType.Attribute)
            {
                return false;
            }
            return this._curNode.MoveToPreviousSibling();
        }

        public override void MoveToRoot()
        {
            this._curNode.MoveToRoot();
        }

        XmlNode IHasXmlNode.GetNode()
        {
            return this._curNode.Node;
        }

        public override string BaseURI
        {
            get
            {
                return this._curNode.BaseURI;
            }
        }

        internal XPathNodePointer CurNode
        {
            get
            {
                return this._curNode;
            }
        }

        internal XmlDataDocument Document
        {
            get
            {
                return this._doc;
            }
        }

        public override bool HasAttributes
        {
            get
            {
                return (this._curNode.AttributeCount > 0);
            }
        }

        public override bool HasChildren
        {
            get
            {
                return this._curNode.HasChildren;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                return this._curNode.IsEmptyElement;
            }
        }

        public override string LocalName
        {
            get
            {
                return this._curNode.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                return this._curNode.Name;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return this._curNode.NamespaceURI;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return this._doc.NameTable;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this._curNode.NodeType;
            }
        }

        public override string Prefix
        {
            get
            {
                return this._curNode.Prefix;
            }
        }

        public override string Value
        {
            get
            {
                XPathNodeType nodeType = this._curNode.NodeType;
                if ((nodeType != XPathNodeType.Element) && (nodeType != XPathNodeType.Root))
                {
                    return this._curNode.Value;
                }
                return this._curNode.InnerText;
            }
        }

        public override string XmlLang
        {
            get
            {
                return this._curNode.XmlLang;
            }
        }
    }
}

