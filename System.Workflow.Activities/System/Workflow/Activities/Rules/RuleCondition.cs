namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public abstract class RuleCondition
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleCondition()
        {
        }

        public abstract RuleCondition Clone();
        public abstract bool Evaluate(RuleExecution execution);
        public abstract ICollection<string> GetDependencies(RuleValidation validation);
        public virtual void OnRuntimeInitialized()
        {
        }

        public abstract bool Validate(RuleValidation validation);

        public abstract string Name { get; set; }
    }
}

