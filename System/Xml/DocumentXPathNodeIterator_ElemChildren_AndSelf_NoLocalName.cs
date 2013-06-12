namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal sealed class DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName : DocumentXPathNodeIterator_ElemChildren_NoLocalName
    {
        internal DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName(DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName other) : base(other)
        {
        }

        internal DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName(DocumentXPathNavigator nav, string nsAtom) : base(nav, nsAtom)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new DocumentXPathNodeIterator_ElemChildren_AndSelf_NoLocalName(this);
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

