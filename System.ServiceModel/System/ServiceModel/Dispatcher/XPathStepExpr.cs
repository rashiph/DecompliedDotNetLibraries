namespace System.ServiceModel.Dispatcher
{
    using System;

    internal class XPathStepExpr : XPathExpr
    {
        private NodeSelectCriteria selectDesc;

        internal XPathStepExpr(NodeSelectCriteria desc) : this(desc, null)
        {
        }

        internal XPathStepExpr(NodeSelectCriteria desc, XPathExprList predicates) : base(XPathExprType.PathStep, ValueDataType.Sequence, predicates)
        {
            this.selectDesc = desc;
        }

        internal NodeSelectCriteria SelectDesc
        {
            get
            {
                return this.selectDesc;
            }
        }
    }
}

