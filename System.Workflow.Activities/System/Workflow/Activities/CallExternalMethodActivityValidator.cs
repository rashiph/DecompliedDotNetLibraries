namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    public class CallExternalMethodActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (!(obj is CallExternalMethodActivity))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CallExternalMethodActivity).FullName }), "obj");
            }
            ValidationErrorCollection errors = base.Validate(manager, obj);
            errors.AddRange(CorrelationSetsValidator.Validate(manager, obj));
            errors.AddRange(ParameterBindingValidator.Validate(manager, obj));
            return errors;
        }
    }
}

