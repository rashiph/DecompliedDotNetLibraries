namespace System.Xml
{
    using System;

    public abstract class XmlLinkedNode : XmlNode
    {
        internal XmlLinkedNode next;

        internal XmlLinkedNode()
        {
            this.next = null;
        }

        internal XmlLinkedNode(XmlDocument doc) : base(doc)
        {
            this.next = null;
        }

        public override XmlNode NextSibling
        {
            get
            {
                XmlNode parentNode = this.ParentNode;
                if ((parentNode != null) && (this.next != parentNode.FirstChild))
                {
                    return this.next;
                }
                return null;
            }
        }

        public override XmlNode PreviousSibling
        {
            get
            {
                XmlNode parentNode = this.ParentNode;
                if (parentNode == null)
                {
                    return null;
                }
                XmlNode firstChild = parentNode.FirstChild;
                while (firstChild != null)
                {
                    XmlNode nextSibling = firstChild.NextSibling;
                    if (nextSibling == this)
                    {
                        return firstChild;
                    }
                    firstChild = nextSibling;
                }
                return firstChild;
            }
        }
    }
}

