namespace System.Xml
{
    using System;
    using System.Collections;

    internal class XmlElementList : XmlNodeList
    {
        private string asterisk;
        private bool atomized;
        private int changeCount;
        private XmlNode curElem;
        private int curInd;
        private bool empty;
        private XmlElementListListener listener;
        private string localName;
        private int matchCount;
        private string name;
        private string namespaceURI;
        private XmlNode rootNode;

        private XmlElementList(XmlNode parent)
        {
            this.rootNode = parent;
            this.curInd = -1;
            this.curElem = this.rootNode;
            this.changeCount = 0;
            this.empty = false;
            this.atomized = true;
            this.matchCount = -1;
            this.listener = new XmlElementListListener(parent.Document, this);
        }

        internal XmlElementList(XmlNode parent, string name) : this(parent)
        {
            XmlNameTable nameTable = parent.Document.NameTable;
            this.asterisk = nameTable.Add("*");
            this.name = nameTable.Add(name);
            this.localName = null;
            this.namespaceURI = null;
        }

        internal XmlElementList(XmlNode parent, string localName, string namespaceURI) : this(parent)
        {
            XmlNameTable nameTable = parent.Document.NameTable;
            this.asterisk = nameTable.Add("*");
            this.localName = nameTable.Get(localName);
            this.namespaceURI = nameTable.Get(namespaceURI);
            if ((this.localName == null) || (this.namespaceURI == null))
            {
                this.empty = true;
                this.atomized = false;
                this.localName = localName;
                this.namespaceURI = namespaceURI;
            }
            this.name = null;
        }

        internal void ConcurrencyCheck(XmlNodeChangedEventArgs args)
        {
            if (!this.atomized)
            {
                XmlNameTable nameTable = this.rootNode.Document.NameTable;
                this.localName = nameTable.Add(this.localName);
                this.namespaceURI = nameTable.Add(this.namespaceURI);
                this.atomized = true;
            }
            if (this.IsMatch(args.Node))
            {
                this.changeCount++;
                this.curInd = -1;
                this.curElem = this.rootNode;
                if (args.Action == XmlNodeChangedAction.Insert)
                {
                    this.empty = false;
                }
            }
            this.matchCount = -1;
        }

        ~XmlElementList()
        {
            if (this.listener != null)
            {
                this.listener.Unregister();
                this.listener = null;
            }
        }

        public override IEnumerator GetEnumerator()
        {
            if (this.empty)
            {
                return new XmlEmptyElementListEnumerator(this);
            }
            return new XmlElementListEnumerator(this);
        }

        private XmlNode GetMatchingNode(XmlNode n, bool bNext)
        {
            XmlNode curNode = n;
            do
            {
                if (bNext)
                {
                    curNode = this.NextElemInPreOrder(curNode);
                }
                else
                {
                    curNode = this.PrevElemInPreOrder(curNode);
                }
            }
            while ((curNode != null) && !this.IsMatch(curNode));
            return curNode;
        }

        public XmlNode GetNextNode(XmlNode n)
        {
            if (this.empty)
            {
                return null;
            }
            XmlNode node = (n == null) ? this.rootNode : n;
            return this.GetMatchingNode(node, true);
        }

        private XmlNode GetNthMatchingNode(XmlNode n, bool bNext, int nCount)
        {
            XmlNode matchingNode = n;
            for (int i = 0; i < nCount; i++)
            {
                matchingNode = this.GetMatchingNode(matchingNode, bNext);
                if (matchingNode == null)
                {
                    return null;
                }
            }
            return matchingNode;
        }

        private bool IsMatch(XmlNode curNode)
        {
            if (curNode.NodeType == XmlNodeType.Element)
            {
                if (this.name != null)
                {
                    if (Ref.Equal(this.name, this.asterisk) || Ref.Equal(curNode.Name, this.name))
                    {
                        return true;
                    }
                }
                else if ((Ref.Equal(this.localName, this.asterisk) || Ref.Equal(curNode.LocalName, this.localName)) && (Ref.Equal(this.namespaceURI, this.asterisk) || (curNode.NamespaceURI == this.namespaceURI)))
                {
                    return true;
                }
            }
            return false;
        }

        public override XmlNode Item(int index)
        {
            if (((this.rootNode != null) && (index >= 0)) && !this.empty)
            {
                if (this.curInd == index)
                {
                    return this.curElem;
                }
                int nCount = index - this.curInd;
                bool bNext = nCount > 0;
                if (nCount < 0)
                {
                    nCount = -nCount;
                }
                XmlNode node = this.GetNthMatchingNode(this.curElem, bNext, nCount);
                if (node != null)
                {
                    this.curInd = index;
                    this.curElem = node;
                    return this.curElem;
                }
            }
            return null;
        }

        private XmlNode NextElemInPreOrder(XmlNode curNode)
        {
            XmlNode firstChild = curNode.FirstChild;
            if (firstChild == null)
            {
                firstChild = curNode;
                while (((firstChild != null) && (firstChild != this.rootNode)) && (firstChild.NextSibling == null))
                {
                    firstChild = firstChild.ParentNode;
                }
                if ((firstChild != null) && (firstChild != this.rootNode))
                {
                    firstChild = firstChild.NextSibling;
                }
            }
            if (firstChild == this.rootNode)
            {
                firstChild = null;
            }
            return firstChild;
        }

        private XmlNode PrevElemInPreOrder(XmlNode curNode)
        {
            XmlNode previousSibling = curNode.PreviousSibling;
            while (previousSibling != null)
            {
                if (previousSibling.LastChild == null)
                {
                    break;
                }
                previousSibling = previousSibling.LastChild;
            }
            if (previousSibling == null)
            {
                previousSibling = curNode.ParentNode;
            }
            if (previousSibling == this.rootNode)
            {
                previousSibling = null;
            }
            return previousSibling;
        }

        internal int ChangeCount
        {
            get
            {
                return this.changeCount;
            }
        }

        public override int Count
        {
            get
            {
                if (this.empty)
                {
                    return 0;
                }
                if (this.matchCount < 0)
                {
                    int num = 0;
                    int changeCount = this.changeCount;
                    XmlNode rootNode = this.rootNode;
                    while ((rootNode = this.GetMatchingNode(rootNode, true)) != null)
                    {
                        num++;
                    }
                    if (changeCount != this.changeCount)
                    {
                        return num;
                    }
                    this.matchCount = num;
                }
                return this.matchCount;
            }
        }
    }
}

