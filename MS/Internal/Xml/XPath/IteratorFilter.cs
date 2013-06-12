namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal class IteratorFilter : XPathNodeIterator
    {
        private XPathNodeIterator innerIterator;
        private string name;
        private int position;

        private IteratorFilter(IteratorFilter it)
        {
            this.innerIterator = it.innerIterator.Clone();
            this.name = it.name;
            this.position = it.position;
        }

        internal IteratorFilter(XPathNodeIterator innerIterator, string name)
        {
            this.innerIterator = innerIterator;
            this.name = name;
        }

        public override XPathNodeIterator Clone()
        {
            return new IteratorFilter(this);
        }

        public override bool MoveNext()
        {
            while (this.innerIterator.MoveNext())
            {
                if (this.innerIterator.Current.LocalName == this.name)
                {
                    this.position++;
                    return true;
                }
            }
            return false;
        }

        public override XPathNavigator Current
        {
            get
            {
                return this.innerIterator.Current;
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

