namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    internal class XPathXsltFunctionExpr : XPathExpr
    {
        private XsltContext context;
        private IXsltContextFunction function;

        internal XPathXsltFunctionExpr(XsltContext context, IXsltContextFunction function, XPathExprList subExpr) : base(XPathExprType.XsltFunction, ConvertTypeFromXslt(function.ReturnType), subExpr)
        {
            this.function = function;
            this.context = context;
        }

        internal static ValueDataType ConvertTypeFromXslt(XPathResultType type)
        {
            switch (type)
            {
                case XPathResultType.Number:
                    return ValueDataType.Double;

                case XPathResultType.String:
                    return ValueDataType.String;

                case XPathResultType.Boolean:
                    return ValueDataType.Boolean;

                case XPathResultType.NodeSet:
                    return ValueDataType.Sequence;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
        }

        internal static XPathResultType ConvertTypeToXslt(ValueDataType type)
        {
            switch (type)
            {
                case ValueDataType.Boolean:
                    return XPathResultType.Boolean;

                case ValueDataType.Double:
                    return XPathResultType.Number;

                case ValueDataType.Sequence:
                    return XPathResultType.NodeSet;

                case ValueDataType.String:
                    return XPathResultType.String;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new QueryCompileException(QueryCompileError.InvalidTypeConversion));
        }

        internal XsltContext Context
        {
            get
            {
                return this.context;
            }
        }

        internal IXsltContextFunction Function
        {
            get
            {
                return this.function;
            }
        }
    }
}

