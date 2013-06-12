namespace System.Xml
{
    using System;
    using System.Xml.XPath;

    internal abstract class DocumentXPathNodeIterator_ElemDescendants : XPathNodeIterator
    {
        private int level;
        private DocumentXPathNavigator nav;
        private int position;

        internal DocumentXPathNodeIterator_ElemDescendants(DocumentXPathNavigator nav)
        {
            this.nav = (DocumentXPathNavigator) nav.Clone();
            this.level = 0;
            this.position = 0;
        }

        internal DocumentXPathNodeIterator_ElemDescendants(DocumentXPathNodeIterator_ElemDescendants other)
        {
            this.nav = (DocumentXPathNavigator) other.nav.Clone();
            this.level = other.level;
            this.position = other.position;
        }

        protected abstract bool Match(XmlNode node);
        public override bool MoveNext()
        {
            XmlNode underlyingObject;
            do
            {
                if (this.nav.MoveToFirstChild())
                {
                    this.level++;
                }
                else if (this.level != 0)
                {
                    while (!this.nav.MoveToNext())
                    {
                        this.level--;
                        if (this.level == 0)
                        {
                            return false;
                        }
                        if (!this.nav.MoveToParent())
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
                underlyingObject = (XmlNode) this.nav.UnderlyingObject;
            }
            while ((underlyingObject.NodeType != XmlNodeType.Element) || !this.Match(underlyingObject));
            this.position++;
            return true;
        }

        protected void SetPosition(int pos)
        {
            this.position = pos;
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.nav;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return this.position;
            }
        }
    }
}

