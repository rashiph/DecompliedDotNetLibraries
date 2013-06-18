namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;

    public abstract class RuleExpressionResult
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleExpressionResult()
        {
        }

        public abstract object Value { get; set; }
    }
}

