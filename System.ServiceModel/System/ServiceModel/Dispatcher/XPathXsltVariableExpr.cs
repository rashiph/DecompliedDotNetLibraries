namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Xml.Xsl;

    internal class XPathXsltVariableExpr : XPathExpr
    {
        private XsltContext context;
        private IXsltContextVariable variable;

        internal XPathXsltVariableExpr(XsltContext context, IXsltContextVariable variable) : base(XPathExprType.XsltVariable, XPathXsltFunctionExpr.ConvertTypeFromXslt(variable.VariableType))
        {
            this.variable = variable;
            this.context = context;
        }

        internal XsltContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal IXsltContextVariable Variable
        {
            get
            {
                return this.variable;
            }
        }
    }
}

