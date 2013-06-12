namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal class XPathNodeList : XmlNodeList
    {
        private bool done;
        private List<XmlNode> list;
        private XPathNodeIterator nodeIterator;
        private static readonly object[] nullparams = new object[0];

        public XPathNodeList(XPathNodeIterator nodeIterator)
        {
            this.nodeIterator = nodeIterator;
            this.list = new List<XmlNode>();
            this.done = false;
        }

        public override IEnumerator GetEnumerator()
        {
            return new XmlNodeListEnumerator(this);
        }

        private XmlNode GetNode(XPathNavigator n)
        {
            IHasXmlNode node = (IHasXmlNode) n;
            return node.GetNode();
        }

        public override XmlNode Item(int index)
        {
            if (this.list.Count <= index)
            {
                this.ReadUntil(index);
            }
            if ((index >= 0) && (this.list.Count > index))
            {
                return this.list[index];
            }
            return null;
        }

        internal int ReadUntil(int index)
        {
            int count = this.list.Count;
            while (!this.done && (count <= index))
            {
                if (this.nodeIterator.MoveNext())
                {
                    XmlNode item = this.GetNode(this.nodeIterator.Current);
                    if (item != null)
                    {
                        this.list.Add(item);
                        count++;
                    }
                }
                else
                {
                    this.done = true;
                    return count;
                }
            }
            return count;
        }

        public override int Count
        {
            get
            {
                if (!this.done)
                {
                    this.ReadUntil(0x7fffffff);
                }
                return this.list.Count;
            }
        }
    }
}

