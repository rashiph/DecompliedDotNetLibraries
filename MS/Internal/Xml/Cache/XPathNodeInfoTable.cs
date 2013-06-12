namespace MS.Internal.Xml.Cache
{
    using System;
    using System.Text;
    using System.Xml.XPath;

    internal sealed class XPathNodeInfoTable
    {
        private const int DefaultTableSize = 0x20;
        private XPathNodeInfoAtom[] hashTable = new XPathNodeInfoAtom[0x20];
        private XPathNodeInfoAtom infoCached;
        private int sizeTable = 0;

        private void AddInfo(XPathNodeInfoAtom info)
        {
            int index = info.GetHashCode() & (this.hashTable.Length - 1);
            info.Next = this.hashTable[index];
            this.hashTable[index] = info;
            this.sizeTable++;
        }

        private XPathNodeInfoAtom Atomize(XPathNodeInfoAtom info)
        {
            XPathNodeInfoAtom next = this.hashTable[info.GetHashCode() & (this.hashTable.Length - 1)];
            while (next != null)
            {
                if (info.Equals(next))
                {
                    info.Next = this.infoCached;
                    this.infoCached = info;
                    return next;
                }
                next = next.Next;
            }
            if (this.sizeTable >= this.hashTable.Length)
            {
                XPathNodeInfoAtom[] hashTable = this.hashTable;
                this.hashTable = new XPathNodeInfoAtom[hashTable.Length * 2];
                for (int i = 0; i < hashTable.Length; i++)
                {
                    XPathNodeInfoAtom atom2;
                    for (next = hashTable[i]; next != null; next = atom2)
                    {
                        atom2 = next.Next;
                        this.AddInfo(next);
                    }
                }
            }
            this.AddInfo(info);
            return info;
        }

        public XPathNodeInfoAtom Create(string localName, string namespaceUri, string prefix, string baseUri, XPathNode[] pageParent, XPathNode[] pageSibling, XPathNode[] pageSimilar, XPathDocument doc, int lineNumBase, int linePosBase)
        {
            XPathNodeInfoAtom infoCached;
            if (this.infoCached == null)
            {
                infoCached = new XPathNodeInfoAtom(localName, namespaceUri, prefix, baseUri, pageParent, pageSibling, pageSimilar, doc, lineNumBase, linePosBase);
            }
            else
            {
                infoCached = this.infoCached;
                this.infoCached = infoCached.Next;
                infoCached.Init(localName, namespaceUri, prefix, baseUri, pageParent, pageSibling, pageSimilar, doc, lineNumBase, linePosBase);
            }
            return this.Atomize(infoCached);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.hashTable.Length; i++)
            {
                builder.AppendFormat("{0,4}: ", i);
                for (XPathNodeInfoAtom atom = this.hashTable[i]; atom != null; atom = atom.Next)
                {
                    if (atom != this.hashTable[i])
                    {
                        builder.Append("\n      ");
                    }
                    builder.Append(atom);
                }
                builder.Append('\n');
            }
            return builder.ToString();
        }
    }
}

