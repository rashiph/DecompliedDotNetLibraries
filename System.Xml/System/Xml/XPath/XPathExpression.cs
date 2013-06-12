namespace System.Xml.XPath
{
    using MS.Internal.Xml.XPath;
    using System;
    using System.Collections;
    using System.Xml;

    public abstract class XPathExpression
    {
        internal XPathExpression()
        {
        }

        public abstract void AddSort(object expr, IComparer comparer);
        public abstract void AddSort(object expr, XmlSortOrder order, XmlCaseOrder caseOrder, string lang, XmlDataType dataType);
        public abstract XPathExpression Clone();
        public static XPathExpression Compile(string xpath)
        {
            return Compile(xpath, null);
        }

        public static XPathExpression Compile(string xpath, IXmlNamespaceResolver nsResolver)
        {
            bool flag;
            CompiledXpathExpr expr = new CompiledXpathExpr(new QueryBuilder().Build(xpath, out flag), xpath, flag);
            if (nsResolver != null)
            {
                expr.SetContext(nsResolver);
            }
            return expr;
        }

        private void PrintQuery(XmlWriter w)
        {
            ((CompiledXpathExpr) this).QueryTree.PrintQuery(w);
        }

        public abstract void SetContext(IXmlNamespaceResolver nsResolver);
        public abstract void SetContext(XmlNamespaceManager nsManager);

        public abstract string Expression { get; }

        public abstract XPathResultType ReturnType { get; }
    }
}

