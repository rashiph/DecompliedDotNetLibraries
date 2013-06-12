namespace System.Xml
{
    using System;
    using System.Collections;

    internal class XmlChildNodes : XmlNodeList
    {
        private XmlNode container;

        public XmlChildNodes(XmlNode container)
        {
            this.container = container;
        }

        public override IEnumerator GetEnumerator()
        {
            if (this.container.FirstChild == null)
            {
                return XmlDocument.EmptyEnumerator;
            }
            return new XmlChildEnumerator(this.container);
        }

        public override XmlNode Item(int i)
        {
            if (i >= 0)
            {
                XmlNode firstChild = this.container.FirstChild;
                while (firstChild != null)
                {
                    if (i == 0)
                    {
                        return firstChild;
                    }
                    firstChild = firstChild.NextSibling;
                    i--;
                }
            }
            return null;
        }

        public override int Count
        {
            get
            {
                int num = 0;
                for (XmlNode node = this.container.FirstChild; node != null; node = node.NextSibling)
                {
                    num++;
                }
                return num;
            }
        }
    }
}

