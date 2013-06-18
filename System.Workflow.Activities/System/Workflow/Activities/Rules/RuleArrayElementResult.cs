namespace System.Workflow.Activities.Rules
{
    using System;

    internal class RuleArrayElementResult : RuleExpressionResult
    {
        private long[] indexerArguments;
        private Array targetArray;

        public RuleArrayElementResult(Array targetArray, long[] indexerArguments)
        {
            if (targetArray == null)
            {
                throw new ArgumentNullException("targetArray");
            }
            if (indexerArguments == null)
            {
                throw new ArgumentNullException("indexerArguments");
            }
            this.targetArray = targetArray;
            this.indexerArguments = indexerArguments;
        }

        public override object Value
        {
            get
            {
                return this.targetArray.GetValue(this.indexerArguments);
            }
            set
            {
                this.targetArray.SetValue(value, this.indexerArguments);
            }
        }
    }
}

