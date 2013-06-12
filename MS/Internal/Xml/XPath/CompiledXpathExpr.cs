namespace MS.Internal.Xml.XPath
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class CompiledXpathExpr : XPathExpression
    {
        private string expr;
        private bool needContext;
        private Query query;

        internal CompiledXpathExpr(Query query, string expression, bool needContext)
        {
            this.query = query;
            this.expr = expression;
            this.needContext = needContext;
        }

        public override void AddSort(object expr, IComparer comparer)
        {
            Query queryTree;
            if (expr is string)
            {
                queryTree = new QueryBuilder().Build((string) expr, out this.needContext);
            }
            else
            {
                if (!(expr is CompiledXpathExpr))
                {
                    throw XPathException.Create("Xp_BadQueryObject");
                }
                queryTree = ((CompiledXpathExpr) expr).QueryTree;
            }
            SortQuery query2 = this.query as SortQuery;
            if (query2 == null)
            {
                this.query = query2 = new SortQuery(this.query);
            }
            query2.AddSort(queryTree, comparer);
        }

        public override void AddSort(object expr, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType)
        {
            this.AddSort(expr, new XPathComparerHelper(order, caseOrder, lang, dataType));
        }

        public virtual void CheckErrors()
        {
        }

        public override XPathExpression Clone()
        {
            return new CompiledXpathExpr(Query.Clone(this.query), this.expr, this.needContext);
        }

        public override void SetContext(IXmlNamespaceResolver nsResolver)
        {
            XsltContext context = nsResolver as XsltContext;
            if (context == null)
            {
                if (nsResolver == null)
                {
                    nsResolver = new XmlNamespaceManager(new NameTable());
                }
                context = new UndefinedXsltContext(nsResolver);
            }
            this.query.SetXsltContext(context);
            this.needContext = false;
        }

        public override void SetContext(XmlNamespaceManager nsManager)
        {
            this.SetContext((IXmlNamespaceResolver) nsManager);
        }

        public override string Expression
        {
            get
            {
                return this.expr;
            }
        }

        internal Query QueryTree
        {
            get
            {
                if (this.needContext)
                {
                    throw XPathException.Create("Xp_NoContext");
                }
                return this.query;
            }
        }

        public override XPathResultType ReturnType
        {
            get
            {
                return this.query.StaticType;
            }
        }

        private class UndefinedXsltContext : XsltContext
        {
            private IXmlNamespaceResolver nsResolver;

            public UndefinedXsltContext(IXmlNamespaceResolver nsResolver) : base(false)
            {
                this.nsResolver = nsResolver;
            }

            public override int CompareDocument(string baseUri, string nextbaseUri)
            {
                return string.CompareOrdinal(baseUri, nextbaseUri);
            }

            public override string LookupNamespace(string prefix)
            {
                if (prefix.Length == 0)
                {
                    return string.Empty;
                }
                string str = this.nsResolver.LookupNamespace(prefix);
                if (str == null)
                {
                    throw XPathException.Create("XmlUndefinedAlias", prefix);
                }
                return str;
            }

            public override bool PreserveWhitespace(XPathNavigator node)
            {
                return false;
            }

            public override IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes)
            {
                throw XPathException.Create("Xp_UndefinedXsltContext");
            }

            public override IXsltContextVariable ResolveVariable(string prefix, string name)
            {
                throw XPathException.Create("Xp_UndefinedXsltContext");
            }

            public override string DefaultNamespace
            {
                get
                {
                    return string.Empty;
                }
            }

            public override bool Whitespace
            {
                get
                {
                    return false;
                }
            }
        }
    }
}

