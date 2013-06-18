namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathNumberExpr : XPathLiteralExpr
    {
        private double literal;

        internal XPathNumberExpr(double literal) : base(XPathExprType.Number, ValueDataType.Double)
        {
            this.literal = literal;
        }

        internal override object Literal
        {
            get
            {
                return this.literal;
            }
        }

        internal double Number
        {
            get
            {
                return this.literal;
            }
        }
    }
}

