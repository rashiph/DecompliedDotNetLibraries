namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class IfElseValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            IfElseActivity activity = obj as IfElseActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(IfElseActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count < 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ConditionalLessThanOneChildren"), 0x50c));
            }
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                if (!(activity2 is IfElseBranchActivity))
                {
                    errors.Add(new ValidationError(SR.GetString("Error_ConditionalDeclNotAllConditionalBranchDecl"), 0x50d));
                    return errors;
                }
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
            if ((activity.ExecutionStatus != ActivityExecutionStatus.Initialized) && (activity.ExecutionStatus != ActivityExecutionStatus.Closed))
            {
                return new ValidationError(SR.GetString("Error_DynamicActivity", new object[] { activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus) }), 260);
            }
            return null;
        }
    }
}

