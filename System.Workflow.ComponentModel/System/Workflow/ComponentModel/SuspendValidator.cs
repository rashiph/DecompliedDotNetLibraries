namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class SuspendValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            SuspendActivity activity = obj as SuspendActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(SuspendActivity).FullName }), "obj");
            }
            for (CompositeActivity activity2 = activity.Parent; activity2 != null; activity2 = activity2.Parent)
            {
                if (activity2.SupportsTransaction)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_SuspendInAtomicScope"), 0x525));
                    return errors;
                }
            }
            return errors;
        }
    }
}

