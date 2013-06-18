namespace System.Workflow.Activities.Rules
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class RuleConditionChangeAction : WorkflowChangeAction
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleConditionChangeAction()
        {
        }

        protected override ValidationErrorCollection ValidateChanges(Activity activity)
        {
            return new ValidationErrorCollection();
        }

        public abstract string ConditionName { get; }
    }
}

