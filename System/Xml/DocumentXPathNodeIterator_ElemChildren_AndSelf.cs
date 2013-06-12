namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal sealed class DocumentXPathNodeIterator_ElemChildren_AndSelf : DocumentXPathNodeIterator_ElemChildren
    {
        internal DocumentXPathNodeIterator_ElemChildren_AndSelf(DocumentXPathNodeIterator_ElemChildren_AndSelf other) : base(other)
        {
        }

        internal DocumentXPathNodeIterator_ElemChildren_AndSelf(DocumentXPathNavigator nav, string localNameAtom, string nsAtom) : base(nav, localNameAtom, nsAtom)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new DocumentXPathNodeIterator_ElemChildren_AndSelf(this);
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

