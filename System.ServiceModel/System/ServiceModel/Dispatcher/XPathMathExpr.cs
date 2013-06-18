namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathMathExpr : XPathConjunctExpr
    {
        private MathOperator op;

        internal XPathMathExpr(MathOperator op, XPathExpr left, XPathExpr right) : base(XPathExprType.Math, ValueDataType.Double, left, right)
        {
            this.op = op;
        }

        internal MathOperator Op
        {
            get
            {
                return this.op;
            }
        }
    }
}

