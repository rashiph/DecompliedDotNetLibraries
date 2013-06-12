namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml.XPath;

    internal sealed class ReversePositionQuery : ForwardPositionQuery
    {
        public ReversePositionQuery(Query input) : base(input)
        {
        }

        private ReversePositionQuery(ReversePositionQuery other) : base((ForwardPositionQuery) other)
        {
        }

        public override XPathNodeIterator Clone()
        {
            return new ReversePositionQuery(this);
        }

        public override int CurrentPosition
        {
            get
            {
                return ((base.outputBuffer.Count - base.count) + 1);
            }
        }

        public override QueryProps Properties
        {
            get
            {
                return (base.Properties | QueryProps.Reverse);
            }
        }
    }
}

