namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal abstract class CacheOutputQuery : Query
    {
        internal Query input;
        protected List<XPathNavigator> outputBuffer;

        protected CacheOutputQuery(CacheOutputQuery other) : base(other)
        {
            this.input = Query.Clone(other.input);
            this.outputBuffer = new List<XPathNavigator>(other.outputBuffer);
            base.count = other.count;
        }

        public CacheOutputQuery(Query input)
        {
            this.input = input;
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
            this.outputBuffer.Clear();
            base.count = 0;
            return this.input.Evaluate(context);
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            this.input.PrintQuery(w);
            w.WriteEndElement();
        }

        public override void Reset()
        {
            base.count = 0;
        }

        public override void SetXsltContext(XsltContext context)
        {
            this.input.SetXsltContext(context);
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

        public override XPathResultType StaticType
        {
            get
            {
                return XPathResultType.NodeSet;
            }
        }
    }
}

