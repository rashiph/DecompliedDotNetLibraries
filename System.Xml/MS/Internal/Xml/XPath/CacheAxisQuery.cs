namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    internal abstract class CacheAxisQuery : BaseAxisQuery
    {
        protected List<XPathNavigator> outputBuffer;

        protected CacheAxisQuery(CacheAxisQuery other) : base((BaseAxisQuery) other)
        {
            this.outputBuffer = new List<XPathNavigator>(other.outputBuffer);
            base.count = other.count;
        }

        public CacheAxisQuery(Query qyInput, string name, string prefix, XPathNodeType typeTest) : base(qyInput, name, prefix, typeTest)
        {
            this.outputBuffer = new List<XPathNavigator>();
            base.count = 0;
        }

        public override XPathNavigator Advance()
        {
            if (base.count < this.outputBuffer.Count)
            {
                return this.outputBuffer[base.count++];
            }
            return null;
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            base.Evaluate(context);
            this.outputBuffer.Clear();
            return this;
        }

        public override void Reset()
        {
            base.count = 0;
        }

        public override int Count
        {
            get
            {
                return this.outputBuffer.Count;
            }
        }

        public override XPathNavigator Current
        {
            get
            {
                if (base.count == 0)
                {
                    return null;
                }
                return this.outputBuffer[base.count - 1];
            }
        }

        public override int CurrentPosition
        {
            get
            {
                return base.count;
            }
        }

        public override QueryProps Properties
        {
            get
            {
                return (QueryProps.Merge | QueryProps.Cached | QueryProps.Count | QueryProps.Position);
            }
        }
    }
}

