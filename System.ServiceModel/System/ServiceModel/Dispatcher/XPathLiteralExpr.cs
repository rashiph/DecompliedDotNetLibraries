namespace System.ServiceModel.Dispatcher
{
    using System;

    internal abstract class XPathLiteralExpr : XPathExpr
    {
        internal XPathLiteralExpr(XPathExprType type, ValueDataType returnType) : base(type, returnType)
        {
        }

        internal override bool IsLiteral
        {
            get
            {
                return true;
            }
        }

        internal abstract object Literal { get; }
    }
}

