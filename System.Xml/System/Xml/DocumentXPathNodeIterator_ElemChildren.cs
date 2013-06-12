namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal class DocumentXPathNodeIterator_ElemChildren : DocumentXPathNodeIterator_ElemDescendants
    {
        protected string localNameAtom;
        protected string nsAtom;

        internal DocumentXPathNodeIterator_ElemChildren(DocumentXPathNodeIterator_ElemChildren other) : base(other)
        {
            this.localNameAtom = other.localNameAtom;
            this.nsAtom = other.nsAtom;
        }

        internal DocumentXPathNodeIterator_ElemChildren(DocumentXPathNavigator nav, string localNameAtom, string nsAtom) : base(nav)
        {
            this.localNameAtom = localNameAtom;
            this.nsAtom = nsAtom;
        }

        public override XPathNodeIterator Clone()
        {
            return new DocumentXPathNodeIterator_ElemChildren(this);
        }

        protected override bool Match(XmlNode node)
        {
            return (Ref.Equal(node.LocalName, this.localNameAtom) && Ref.Equal(node.NamespaceURI, this.nsAtom));
        }
    }
}

