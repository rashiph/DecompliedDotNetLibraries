namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal sealed class DocumentXPathNodeIterator_AllElemChildren_AndSelf : DocumentXPathNodeIterator_AllElemChildren
    {
        internal DocumentXPathNodeIterator_AllElemChildren_AndSelf(DocumentXPathNavigator nav) : base(nav)
        {
        }

        internal DocumentXPathNodeIterator_AllElemChildren_AndSelf(DocumentXPathNodeIterator_AllElemChildren_AndSelf other) : base(other)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new DocumentXPathNodeIterator_AllElemChildren_AndSelf(this);
        }

        public override bool MoveNext()
        {
            if (this.CurrentPosition == 0)
            {
                DocumentXPathNavigator current = (DocumentXPathNavigator) this.Current;
                XmlNode underlyingObject = (XmlNode) current.UnderlyingObject;
                if ((underlyingObject.NodeType == XmlNodeType.Element) && this.Match(underlyingObject))
                {
                    base.SetPosition(1);
                    return true;
                }
            }
            return base.MoveNext();
        }
    }
}

