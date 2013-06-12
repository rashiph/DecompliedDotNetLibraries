namespace System.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class RegionIterator : BaseRegionIterator
    {
        private XmlNode currentNode;
        private XmlBoundElement rowElement;

        internal RegionIterator(XmlBoundElement rowElement) : base(((XmlDataDocument) rowElement.OwnerDocument).Mapper)
        {
            this.rowElement = rowElement;
            this.currentNode = rowElement;
        }

        private static string GetInitialTextFromNodes(ref XmlNode n)
        {
            string str = null;
            if (n != null)
            {
                while (n.NodeType == XmlNodeType.Whitespace)
                {
                    n = n.NextSibling;
                    if (n == null)
                    {
                        return string.Empty;
                    }
                }
                if (XmlDataDocument.IsTextLikeNode(n) && ((n.NextSibling == null) || !XmlDataDocument.IsTextLikeNode(n.NextSibling)))
                {
                    str = n.Value;
                    n = n.NextSibling;
                }
                else
                {
                    StringBuilder builder = new StringBuilder();
                    while ((n != null) && XmlDataDocument.IsTextLikeNode(n))
                    {
                        if (n.NodeType != XmlNodeType.Whitespace)
                        {
                            builder.Append(n.Value);
                        }
                        n = n.NextSibling;
                    }
                    str = builder.ToString();
                }
            }
            if (str == null)
            {
                str = string.Empty;
            }
            return str;
        }

        internal override bool Next()
        {
            ElementState elementState = this.rowElement.ElementState;
            XmlNode firstChild = this.currentNode.FirstChild;
            if (firstChild != null)
            {
                this.currentNode = firstChild;
                this.rowElement.ElementState = elementState;
                return true;
            }
            return this.NextRight();
        }

        internal bool NextInitialTextLikeNodes(out string value)
        {
            ElementState elementState = this.rowElement.ElementState;
            XmlNode firstChild = this.CurrentNode.FirstChild;
            value = GetInitialTextFromNodes(ref firstChild);
            if (firstChild == null)
            {
                this.rowElement.ElementState = elementState;
                return this.NextRight();
            }
            this.currentNode = firstChild;
            this.rowElement.ElementState = elementState;
            return true;
        }

        internal override bool NextRight()
        {
            if (this.currentNode == this.rowElement)
            {
                this.currentNode = null;
                return false;
            }
            ElementState elementState = this.rowElement.ElementState;
            XmlNode nextSibling = this.currentNode.NextSibling;
            if (nextSibling != null)
            {
                this.currentNode = nextSibling;
                this.rowElement.ElementState = elementState;
                return true;
            }
            nextSibling = this.currentNode;
            while ((nextSibling != this.rowElement) && (nextSibling.NextSibling == null))
            {
                nextSibling = nextSibling.ParentNode;
            }
            if (nextSibling == this.rowElement)
            {
                this.currentNode = null;
                this.rowElement.ElementState = elementState;
                return false;
            }
            this.currentNode = nextSibling.NextSibling;
            this.rowElement.ElementState = elementState;
            return true;
        }

        internal override void Reset()
        {
            this.currentNode = this.rowElement;
        }

        internal override XmlNode CurrentNode
        {
            get
            {
                return this.currentNode;
            }
        }
    }
}

