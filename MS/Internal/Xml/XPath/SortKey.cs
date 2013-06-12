namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Reflection;
    using System.Xml.XPath;

    internal sealed class SortKey
    {
        private object[] keys;
        private XPathNavigator node;
        private int numKeys;
        private int originalPosition;

        public SortKey(int numKeys, int originalPosition, XPathNavigator node)
        {
            this.numKeys = numKeys;
            this.keys = new object[numKeys];
            this.originalPosition = originalPosition;
            this.node = node;
        }

        public object this[int index]
        {
            get
            {
                return this.keys[index];
            }
            set
            {
                this.keys[index] = value;
            }
        }

        public XPathNavigator Node
        {
            get
            {
                return this.node;
            }
        }

        public int NumKeys
        {
            get
            {
                return this.numKeys;
            }
        }

        public int OriginalPosition
        {
            get
            {
                return this.originalPosition;
            }
        }
    }
}

