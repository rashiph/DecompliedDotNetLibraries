namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class WorkflowChangeAction
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowChangeAction()
        {
        }

        protected internal abstract bool ApplyTo(Activity rootActivity);
        protected internal abstract ValidationErrorCollection ValidateChanges(Activity activity);
    }
}

