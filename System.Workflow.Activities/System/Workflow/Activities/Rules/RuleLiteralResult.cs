namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    public class RuleLiteralResult : RuleExpressionResult
    {
        private object literal;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleLiteralResult(object literal)
        {
            this.literal = literal;
        }

        public override object Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.literal;
            }
            set
            {
                throw new InvalidOperationException(Messages.CannotWriteToExpression);
            }
        }
    }
}

