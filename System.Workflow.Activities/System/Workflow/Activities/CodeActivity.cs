namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;

    [ToolboxBitmap(typeof(CodeActivity), "Resources.code.png"), ActivityValidator(typeof(CodeActivity.CodeActivityValidator)), SRDescription("CodeActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(CodeDesigner), typeof(IDesigner)), DefaultEvent("ExecuteCode"), SRCategory("Standard")]
    public sealed class CodeActivity : Activity
    {
        public static readonly DependencyProperty ExecuteCodeEvent = DependencyProperty.Register("ExecuteCode", typeof(EventHandler), typeof(CodeActivity));

        [SRCategory("Handlers"), MergableProperty(false), SRDescription("UserCodeHandlerDescr")]
        public event EventHandler ExecuteCode
        {
            add
            {
                base.AddHandler(ExecuteCodeEvent, value);
            }
            remove
            {
                base.RemoveHandler(ExecuteCodeEvent, value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CodeActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CodeActivity(string name) : base(name)
        {
        }

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            base.RaiseEvent(ExecuteCodeEvent, this, EventArgs.Empty);
            return ActivityExecutionStatus.Closed;
        }

        private class CodeActivityValidator : ActivityValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();
                CodeActivity activity = obj as CodeActivity;
                if (activity == null)
                {
                    throw new InvalidOperationException();
                }
                if ((activity.GetInvocationList<EventHandler>(CodeActivity.ExecuteCodeEvent).Length == 0) && (activity.GetBinding(CodeActivity.ExecuteCodeEvent) == null))
                {
                    Hashtable hashtable = activity.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                    if ((hashtable == null) || (hashtable["ExecuteCode"] == null))
                    {
                        errors.Add(ValidationError.GetNotSetValidationError("ExecuteCode"));
                    }
                }
                errors.AddRange(base.Validate(manager, obj));
                return errors;
            }
        }
    }
}

