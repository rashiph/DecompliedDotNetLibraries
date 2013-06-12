namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [DebuggerDisplay("{ToString()}")]
    internal abstract class Query : ResetableIterator
    {
        public const XPathResultType XPathResultType_Navigator = ((XPathResultType) 4);

        public Query()
        {
        }

        protected Query(Query other) : base(other)
        {
        }

        public abstract XPathNavigator Advance();
        [Conditional("DEBUG")]
        private void AssertDOD(List<XPathNavigator> buffer, XPathNavigator nav, int pos)
        {
            if ((nav.GetType().ToString() != "Microsoft.VisualStudio.Modeling.StoreNavigator") && (nav.GetType().ToString() != "System.Xml.DataDocumentXPathNavigator"))
            {
                if (0 < pos)
                {
                    CompareNodes(buffer[pos - 1], nav);
                }
                if (pos < buffer.Count)
                {
                    CompareNodes(nav, buffer[pos]);
                }
            }
        }

        [Conditional("DEBUG")]
        public static void AssertQuery(Query query)
        {
            if (!(query is FunctionQuery))
            {
                XPathNavigator navigator2;
                query = Clone(query);
                XPathNavigator l = null;
                int count = query.Clone().Count;
                for (int i = 0; (navigator2 = query.Advance()) != null; i++)
                {
                    if (navigator2.GetType().ToString() == "Microsoft.VisualStudio.Modeling.StoreNavigator")
                    {
                        return;
                    }
                    if (navigator2.GetType().ToString() == "System.Xml.DataDocumentXPathNavigator")
                    {
                        return;
                    }
                    if ((l != null) && ((l.NodeType != XPathNodeType.Namespace) || (navigator2.NodeType != XPathNodeType.Namespace)))
                    {
                        CompareNodes(l, navigator2);
                    }
                    l = navigator2.Clone();
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static Query Clone(Query input)
        {
            if (input != null)
            {
                return (Query) input.Clone();
            }
            return null;
        }

        protected static XPathNavigator Clone(XPathNavigator input)
        {
            if (input != null)
            {
                return input.Clone();
            }
            return null;
        }

        protected static XPathNodeIterator Clone(XPathNodeIterator input)
        {
            if (input != null)
            {
                return input.Clone();
            }
            return null;
        }

        public static XmlNodeOrder CompareNodes(XPathNavigator l, XPathNavigator r)
        {
            XmlNodeOrder order = l.ComparePosition(r);
            if (order != XmlNodeOrder.Unknown)
            {
                return order;
            }
            XPathNavigator navigator = l.Clone();
            navigator.MoveToRoot();
            string baseURI = navigator.BaseURI;
            if (!navigator.MoveTo(r))
            {
                navigator = r.Clone();
            }
            navigator.MoveToRoot();
            string strB = navigator.BaseURI;
            int num = string.CompareOrdinal(baseURI, strB);
            return ((num < 0) ? XmlNodeOrder.Before : ((num > 0) ? XmlNodeOrder.After : XmlNodeOrder.Unknown));
        }

        public abstract object Evaluate(XPathNodeIterator nodeIterator);
        private static int GetMedian(int l, int r)
        {
            return ((l + r) >> 1);
        }

        protected XPathResultType GetXPathType(object value)
        {
            if (value is XPathNodeIterator)
            {
                return XPathResultType.NodeSet;
            }
            if (value is string)
            {
                return XPathResultType.String;
            }
            if (value is double)
            {
                return XPathResultType.Number;
            }
            if (value is bool)
            {
                return XPathResultType.Boolean;
            }
            return (XPathResultType) 4;
        }

        public bool Insert(List<XPathNavigator> buffer, XPathNavigator nav)
        {
            int l = 0;
            int count = buffer.Count;
            if (count != 0)
            {
                switch (CompareNodes(buffer[count - 1], nav))
                {
                    case XmlNodeOrder.Before:
                        buffer.Add(nav.Clone());
                        return true;

                    case XmlNodeOrder.Same:
                        return false;
                }
                count--;
            }
            while (l < count)
            {
                int median = GetMedian(l, count);
                switch (CompareNodes(buffer[median], nav))
                {
                    case XmlNodeOrder.Before:
                    {
                        l = median + 1;
                        continue;
                    }
                    case XmlNodeOrder.Same:
                        return false;
                }
                count = median;
            }
            buffer.Insert(l, nav.Clone());
            return true;
        }

        public virtual XPathNavigator MatchNode(XPathNavigator current)
        {
            throw XPathException.Create("Xp_InvalidPattern");
        }

        public override bool MoveNext()
        {
            return (this.Advance() != null);
        }

        public virtual void PrintQuery(XmlWriter w)
        {
            w.WriteElementString(base.GetType().Name, string.Empty);
        }

        public virtual void SetXsltContext(XsltContext context)
        {
        }

        public override int Count
        {
            get
            {
                if (base.count == -1)
                {
                    Query query = (Query) this.Clone();
                    query.Reset();
                    base.count = 0;
                    while (query.MoveNext())
                    {
                        base.count++;
                    }
                }
                return base.count;
            }
        }

        public virtual QueryProps Properties
        {
            get
            {
                return QueryProps.Merge;
            }
        }

        public abstract XPathResultType StaticType { get; }

        public virtual double XsltDefaultPriority
        {
            get
            {
                return 0.5;
            }
        }
    }
}

