namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathRelationExpr : XPathConjunctExpr
    {
        private RelationOperator op;

        internal XPathRelationExpr(RelationOperator op, XPathExpr left, XPathExpr right) : base(XPathExprType.Relational, ValueDataType.Boolean, left, right)
        {
            this.op = op;
        }

        internal RelationOperator Op
        {
            get
            {
                return this.op;
            }
            set
            {
                this.op = value;
            }
        }
    }
}

