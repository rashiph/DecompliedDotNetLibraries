namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class XPathEmptyIterator : ResetableIterator
    {
        public static XPathEmptyIterator Instance = new XPathEmptyIterator();

        private XPathEmptyIterator()
        {
        }

        public override XPathNodeIterator Clone()
        {
            return this;
        }

        public override bool MoveNext()
        {
            return false;
        }

        public override void Reset()
        {
        }

        public override int Count
        {
            get
            {
                return 0;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                return null;
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return 0;
            }
        }
    }
}

