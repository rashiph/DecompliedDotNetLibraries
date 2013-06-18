namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    public class RuleExpressionInfo
    {
        private Type expressionType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleExpressionInfo(Type expressionType)
        {
            this.expressionType = expressionType;
        }

        public Type ExpressionType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.expressionType;
            }
        }
    }
}

