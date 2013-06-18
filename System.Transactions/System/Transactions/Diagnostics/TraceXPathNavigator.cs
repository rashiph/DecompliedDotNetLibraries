namespace System.Transactions.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Transactions;
    using System.Xml;
    using System.Xml.XPath;

    internal class TraceXPathNavigator : XPathNavigator
    {
        private bool closed;
        private ElementNode current;
        private ElementNode root;
        private XPathNodeType state = XPathNodeType.Element;

        internal void AddAttribute(string name, string value, string xmlns, string prefix)
        {
            if (this.closed)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("CannotAddToClosedDocument"));
            }
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            AttributeNode item = new AttributeNode(name, prefix, xmlns, value);
            this.current.attributes.Add(item);
        }

        internal void AddElement(string prefix, string name, string xmlns)
        {
            ElementNode item = new ElementNode(name, prefix, xmlns, this.current);
            if (this.closed)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("CannotAddToClosedDocument"));
            }
            if (this.current == null)
            {
                this.root = item;
                this.current = this.root;
            }
            else if (!this.closed)
            {
                this.current.childNodes.Add(item);
                this.current = item;
            }
        }

        internal void AddText(string value)
        {
            if (this.closed)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("CannotAddToClosedDocument"));
            }
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            if (this.current.text != null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("TextNodeAlreadyPopulated"));
            }
            this.current.text = new TextNode(value);
        }

        public override XPathNavigator Clone()
        {
            return this;
        }

        internal void CloseElement()
        {
            if (this.closed)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("DocumentAlreadyClosed"));
            }
            this.current = this.current.parent;
            if (this.current == null)
            {
                this.closed = true;
            }
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            throw new NotSupportedException();
        }

        public override bool MoveTo(XPathNavigator other)
        {
            throw new NotSupportedException();
        }

        public override bool MoveToFirstAttribute()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            bool flag = this.current.MoveToFirstAttribute();
            if (flag)
            {
                this.state = XPathNodeType.Attribute;
            }
            return flag;
        }

        public override bool MoveToFirstChild()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            bool flag = false;
            if (this.current.childNodes.Count > 0)
            {
                this.current = this.current.childNodes[0];
                this.state = XPathNodeType.Element;
                return true;
            }
            if ((this.current.childNodes.Count == 0) && (this.current.text != null))
            {
                this.state = XPathNodeType.Text;
                this.current.movedToText = true;
                flag = true;
            }
            return flag;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            throw new NotSupportedException();
        }

        public override bool MoveToNext()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            bool flag = false;
            if (this.state != XPathNodeType.Text)
            {
                ElementNode parent = this.current.parent;
                if (parent == null)
                {
                    return flag;
                }
                ElementNode node2 = parent.MoveToNext();
                if (((node2 == null) && (parent.text != null)) && !parent.movedToText)
                {
                    this.state = XPathNodeType.Text;
                    parent.movedToText = true;
                    return true;
                }
                if (node2 != null)
                {
                    this.state = XPathNodeType.Element;
                    flag = true;
                    this.current = node2;
                }
            }
            return flag;
        }

        public override bool MoveToNextAttribute()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            bool flag = this.current.MoveToNextAttribute();
            if (flag)
            {
                this.state = XPathNodeType.Attribute;
            }
            return flag;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            if (this.current == null)
            {
                throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
            }
            bool flag = false;
            switch (this.state)
            {
                case XPathNodeType.Element:
                    if (this.current.parent != null)
                    {
                        this.current = this.current.parent;
                        this.state = XPathNodeType.Element;
                        flag = true;
                    }
                    return flag;

                case XPathNodeType.Attribute:
                    this.state = XPathNodeType.Element;
                    return true;

                case XPathNodeType.Namespace:
                    this.state = XPathNodeType.Element;
                    return true;

                case XPathNodeType.Text:
                    this.state = XPathNodeType.Element;
                    return true;
            }
            return flag;
        }

        public override bool MoveToPrevious()
        {
            throw new NotSupportedException();
        }

        public override void MoveToRoot()
        {
            this.current = this.root;
            this.state = XPathNodeType.Element;
            this.root.Reset();
        }

        public override string ToString()
        {
            this.MoveToRoot();
            StringBuilder sb = new StringBuilder();
            new XmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture)).WriteNode(this, false);
            return sb.ToString();
        }

        public override string BaseURI
        {
            get
            {
                return null;
            }
        }

        public override bool IsEmptyElement
        {
            get
            {
                bool flag = true;
                if (this.current != null)
                {
                    flag = (this.current.text != null) || (this.current.childNodes.Count > 0);
                }
                return flag;
            }
        }

        public override string LocalName
        {
            get
            {
                return this.Name;
            }
        }

        public override string Name
        {
            get
            {
                if (this.current == null)
                {
                    throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
                }
                switch (this.state)
                {
                    case XPathNodeType.Element:
                        return this.current.name;

                    case XPathNodeType.Attribute:
                        return this.current.CurrentAttribute.name;
                }
                return null;
            }
        }

        public override string NamespaceURI
        {
            get
            {
                return null;
            }
        }

        public override XmlNameTable NameTable
        {
            get
            {
                return null;
            }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                return this.state;
            }
        }

        public override string Prefix
        {
            get
            {
                if (this.current == null)
                {
                    throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
                }
                switch (this.state)
                {
                    case XPathNodeType.Element:
                        return this.current.prefix;

                    case XPathNodeType.Attribute:
                        return this.current.CurrentAttribute.prefix;

                    case XPathNodeType.Namespace:
                        return this.current.prefix;
                }
                return null;
            }
        }

        public override string Value
        {
            get
            {
                if (this.current == null)
                {
                    throw new InvalidOperationException(System.Transactions.SR.GetString("OperationInvalidOnAnEmptyDocument"));
                }
                switch (this.state)
                {
                    case XPathNodeType.Attribute:
                        return this.current.CurrentAttribute.nodeValue;

                    case XPathNodeType.Namespace:
                        return this.current.xmlns;

                    case XPathNodeType.Text:
                        return this.current.text.nodeValue;
                }
                return null;
            }
        }

        private class AttributeNode
        {
            internal string name;
            internal string nodeValue;
            internal string prefix;
            internal string xmlns;

            internal AttributeNode(string name, string prefix, string xmlns, string value)
            {
                this.name = name;
                this.prefix = prefix;
                this.xmlns = xmlns;
                this.nodeValue = value;
            }
        }

        private class ElementNode
        {
            private int attributeIndex;
            internal List<TraceXPathNavigator.AttributeNode> attributes = new List<TraceXPathNavigator.AttributeNode>();
            internal List<TraceXPathNavigator.ElementNode> childNodes = new List<TraceXPathNavigator.ElementNode>();
            private int elementIndex;
            internal bool movedToText;
            internal string name;
            internal TraceXPathNavigator.ElementNode parent;
            internal string prefix;
            internal TraceXPathNavigator.TextNode text;
            internal string xmlns;

            internal ElementNode(string name, string prefix, string xmlns, TraceXPathNavigator.ElementNode parent)
            {
                this.name = name;
                this.prefix = prefix;
                this.xmlns = xmlns;
                this.parent = parent;
            }

            internal bool MoveToFirstAttribute()
            {
                this.attributeIndex = 0;
                return (this.attributes.Count > 0);
            }

            internal TraceXPathNavigator.ElementNode MoveToNext()
            {
                TraceXPathNavigator.ElementNode node = null;
                if ((this.elementIndex + 1) < this.childNodes.Count)
                {
                    this.elementIndex++;
                    node = this.childNodes[this.elementIndex];
                }
                return node;
            }

            internal bool MoveToNextAttribute()
            {
                bool flag = false;
                if ((this.attributeIndex + 1) < this.attributes.Count)
                {
                    this.attributeIndex++;
                    flag = true;
                }
                return flag;
            }

            internal void Reset()
            {
                this.attributeIndex = 0;
                this.elementIndex = 0;
                foreach (TraceXPathNavigator.ElementNode node in this.childNodes)
                {
                    node.Reset();
                }
            }

            internal TraceXPathNavigator.AttributeNode CurrentAttribute
            {
                get
                {
                    return this.attributes[this.attributeIndex];
                }
            }
        }

        private class TextNode
        {
            internal string nodeValue;

            internal TextNode(string value)
            {
                this.nodeValue = value;
            }
        }
    }
}

