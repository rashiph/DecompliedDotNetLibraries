namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public abstract class RuleAction
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleAction()
        {
        }

        public abstract RuleAction Clone();
        public abstract void Execute(RuleExecution context);
        public abstract ICollection<string> GetSideEffects(RuleValidation validation);
        public abstract bool Validate(RuleValidation validator);
    }
}

