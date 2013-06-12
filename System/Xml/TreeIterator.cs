namespace System.Xml
{
    using System;

    internal sealed class TreeIterator : BaseTreeIterator
    {
        private XmlNode currentNode;
        private XmlNode nodeTop;

        internal TreeIterator(XmlNode nodeTop) : base(((XmlDataDocument) nodeTop.OwnerDocument).Mapper)
        {
            this.nodeTop = nodeTop;
            this.currentNode = nodeTop;
        }

        internal override bool Next()
        {
            XmlNode firstChild = this.currentNode.FirstChild;
            if (firstChild != null)
            {
                this.currentNode = firstChild;
                return true;
            }
            return this.NextRight();
        }

        internal override bool NextRight()
        {
            if (this.currentNode == this.nodeTop)
            {
                this.currentNode = null;
                return false;
            }
            XmlNode nextSibling = this.currentNode.NextSibling;
            if (nextSibling != null)
            {
                this.currentNode = nextSibling;
                return true;
            }
            nextSibling = this.currentNode;
            while ((nextSibling != this.nodeTop) && (nextSibling.NextSibling == null))
            {
                nextSibling = nextSibling.ParentNode;
            }
            if (nextSibling == this.nodeTop)
            {
                this.currentNode = null;
                return false;
            }
            this.currentNode = nextSibling.NextSibling;
            return true;
        }

        internal override void Reset()
        {
            this.currentNode = this.nodeTop;
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

