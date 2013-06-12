namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class EmptyQuery : Query
    {
        public override XPathNavigator Advance()
        {
            return null;
        }

        public override XPathNodeIterator Clone()
        {
            return this;
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            return this;
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

        public override QueryProps Properties
        {
            get
            {
                return (QueryProps.Merge | QueryProps.Cached | QueryProps.Count | QueryProps.Position);
            }
        }

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }
    }
}

