namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class CompensationValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            CompensationHandlerActivity activity = obj as CompensationHandlerActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CompensationHandlerActivity).FullName }), "obj");
            }
            if (!(activity.Parent is ICompensatableActivity))
            {
                errors.Add(new ValidationError(SR.GetString("Error_ParentDoesNotSupportCompensation"), 0x519));
            }
            if (activity.EnabledActivities.Count == 0)
            {
                errors.Add(new ValidationError(SR.GetString("Warning_EmptyBehaviourActivity", new object[] { typeof(CompensationHandlerActivity).FullName, activity.QualifiedName }), 0x1a3, true));
                return errors;
            }
            if (activity.AlternateFlowActivities.Count > 0)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ModelingConstructsCanNotContainModelingConstructs"), 0x61f));
            }
            return errors;
        }
    }
}

