namespace System.Xml.Xsl
{
    using System;
    using System.Xml;
    using System.Xml.XPath;

    public abstract class XsltContext : XmlNamespaceManager
    {
        protected XsltContext() : base(new NameTable())
        {
        }

        internal XsltContext(bool dummy)
        {
        }

        protected XsltContext(NameTable table) : base(table)
        {
        }

        public abstract int CompareDocument(string baseUri, string nextbaseUri);
        public abstract bool PreserveWhitespace(XPathNavigator node);
        public abstract IXsltContextFunction ResolveFunction(string prefix, string name, XPathResultType[] ArgTypes);
        public abstract IXsltContextVariable ResolveVariable(string prefix, string name);

        public abstract bool Whitespace { get; }
    }
}

