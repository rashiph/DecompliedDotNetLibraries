namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class WhileValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection(base.Validate(manager, obj));
            WhileActivity activity = obj as WhileActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(WhileActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count != 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_WhileShouldHaveOneChild"), 0x60b));
            }
            return errors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            if (((activity.ExecutionStatus != ActivityExecutionStatus.Initialized) && (activity.ExecutionStatus != ActivityExecutionStatus.Executing)) && (activity.ExecutionStatus != ActivityExecutionStatus.Closed))
            {
                return new ValidationError(SR.GetString("Error_DynamicActivity2", new object[] { activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus), activity.GetType().FullName }), 0x50f);
            }
            return null;
        }
    }
}

