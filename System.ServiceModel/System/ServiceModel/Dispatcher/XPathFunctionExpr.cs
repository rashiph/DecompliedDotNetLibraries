namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathFunctionExpr : XPathExpr
    {
        private QueryFunction function;

        internal XPathFunctionExpr(QueryFunction function, XPathExprList subExpr) : base(XPathExprType.Function, function.ReturnType, subExpr)
        {
            this.function = function;
        }

        internal QueryFunction Function
        {
            get
            {
                return this.function;
            }
        }
    }
}

