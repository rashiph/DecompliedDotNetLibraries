namespace System.Xml.Xsl
{
    using System;
    using System.Xml.XPath;

    public interface IXsltContextFunction
    {
        object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext);

        XPathResultType[] ArgTypes { get; }

        int Maxargs { get; }

        int Minargs { get; }

        XPathResultType ReturnType { get; }
    }
}

