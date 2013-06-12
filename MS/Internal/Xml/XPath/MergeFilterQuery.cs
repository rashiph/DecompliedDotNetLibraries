namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class MergeFilterQuery : CacheOutputQuery
    {
        private Query child;

        private MergeFilterQuery(MergeFilterQuery other) : base((CacheOutputQuery) other)
        {
            this.child = Query.Clone(other.child);
        }

        public MergeFilterQuery(Query input, Query child) : base(input)
        {
            this.child = child;
        }

        public override XPathNodeIterator Clone()
        {
            return new MergeFilterQuery(this);
        }

        public override object Evaluate(XPathNodeIterator nodeIterator)
        {
            base.Evaluate(nodeIterator);
            while (base.input.Advance() != null)
            {
                XPathNavigator navigator;
                this.child.Evaluate(base.input);
                while ((navigator = this.child.Advance()) != null)
                {
                    base.Insert(base.outputBuffer, navigator);
                }
            }
            return this;
        }

        public override XPathNavigator MatchNode(XPathNavigator current)
        {
            XPathNavigator navigator = this.child.MatchNode(current);
            if (navigator != null)
            {
                navigator = base.input.MatchNode(navigator);
                if (navigator == null)
                {
                    return null;
                }
                this.Evaluate(new XPathSingletonIterator(navigator.Clone(), true));
                for (XPathNavigator navigator2 = this.Advance(); navigator2 != null; navigator2 = this.Advance())
                {
                    if (navigator2.IsSamePosition(current))
                    {
                        return navigator;
                    }
                }
            }
            return null;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            base.input.PrintQuery(w);
            this.child.PrintQuery(w);
            w.WriteEndElement();
        }

        public override void SetXsltContext(XsltContext xsltContext)
        {
            base.SetXsltContext(xsltContext);
            this.child.SetXsltContext(xsltContext);
        }
    }
}

