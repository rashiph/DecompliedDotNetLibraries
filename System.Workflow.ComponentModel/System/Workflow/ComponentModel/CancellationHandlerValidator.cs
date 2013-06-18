namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class CancellationHandlerValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            CancellationHandlerActivity activity = obj as CancellationHandlerActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CancellationHandlerActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count == 0)
            {
                errors.Add(new ValidationError(SR.GetString("Warning_EmptyBehaviourActivity", new object[] { typeof(CancellationHandlerActivity).FullName, activity.QualifiedName }), 0x1a3, true));
            }
            if (activity.AlternateFlowActivities.Count > 0)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ModelingConstructsCanNotContainModelingConstructs"), 0x61f));
            }
            return errors;
        }
    }
}

