namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ReplicatorValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            ReplicatorActivity activity = obj as ReplicatorActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ReplicatorActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count != 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_GeneratorShouldContainSingleActivity"), 0x526));
            }
            return errors;
        }
    }
}

