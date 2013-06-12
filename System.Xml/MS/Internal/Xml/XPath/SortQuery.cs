namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal sealed class SortQuery : Query
    {
        private XPathSortComparer comparer;
        private Query qyInput;
        private List<SortKey> results;

        public SortQuery(Query qyInput)
        {
            this.results = new List<SortKey>();
            this.comparer = new XPathSortComparer();
            this.qyInput = qyInput;
            base.count = 0;
        }

        private SortQuery(SortQuery other) : base(other)
        {
            this.results = new List<SortKey>(other.results);
            this.comparer = other.comparer.Clone();
            this.qyInput = Query.Clone(other.qyInput);
            base.count = 0;
        }

        internal void AddSort(Query evalQuery, IComparer comparer)
        {
            this.comparer.AddSort(evalQuery, comparer);
        }

        public override XPathNavigator Advance()
        {
            if (base.count < this.results.Count)
            {
                return this.results[base.count++].Node;
            }
            return null;
        }

        private void BuildResultsList()
        {
            XPathNavigator navigator;
            int numSorts = this.comparer.NumSorts;
            while ((navigator = this.qyInput.Advance()) != null)
            {
                SortKey item = new SortKey(numSorts, this.results.Count, navigator.Clone());
                for (int i = 0; i < numSorts; i++)
                {
                    item[i] = this.comparer.Expression(i).Evaluate(this.qyInput);
                }
                this.results.Add(item);
            }
            this.results.Sort(this.comparer);
        }

        public override XPathNodeIterator Clone()
        {
            return new SortQuery(this);
        }

        public override object Evaluate(XPathNodeIterator context)
        {
            this.qyInput.Evaluate(context);
            this.results.Clear();
            this.BuildResultsList();
            base.count = 0;
            return this;
        }

        public override void PrintQuery(XmlWriter w)
        {
            w.WriteStartElement(base.GetType().Name);
            this.qyInput.PrintQuery(w);
            w.WriteElementString("XPathSortComparer", "... PrintTree() not implemented ...");
            w.WriteEndElement();
        }

        public override void Reset()
        {
            base.count = 0;
        }

        public override void SetXsltContext(XsltContext xsltContext)
        {
            this.qyInput.SetXsltContext(xsltContext);
            if ((this.qyInput.StaticType != XPathResultType.NodeSet) && (this.qyInput.StaticType != XPathResultType.Any))
            {
                throw XPathException.Create("Xp_NodeSetExpected");
            }
        }

        public override int Count
        {
            get
            {
                return this.results.Count;
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
                return this.results[base.count - 1].Node;
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
                return (QueryProps.Cached | QueryProps.Count | QueryProps.Position);
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

