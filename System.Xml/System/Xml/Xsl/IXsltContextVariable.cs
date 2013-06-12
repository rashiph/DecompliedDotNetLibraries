namespace System.Xml.Xsl
{
    using System;
    using System.Xml.XPath;

    public interface IXsltContextVariable
    {
        object Evaluate(XsltContext xsltContext);

        bool IsLocal { get; }

        bool IsParam { get; }

        XPathResultType VariableType { get; }
    }
}

