namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class IfElseBranchValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            IfElseBranchActivity activity = obj as IfElseBranchActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(IfElseBranchActivity).FullName }), "obj");
            }
            IfElseActivity parent = activity.Parent as IfElseActivity;
            if (parent == null)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ConditionalBranchParentNotConditional"), 0x50e));
            }
            if (((((parent == null) || (parent.EnabledActivities.Count <= 1)) || (parent.EnabledActivities[parent.EnabledActivities.Count - 1] != activity)) || (activity.Condition != null)) && (activity.Condition == null))
            {
                errors.Add(ValidationError.GetNotSetValidationError("Condition"));
            }
            return errors;
        }
    }
}

