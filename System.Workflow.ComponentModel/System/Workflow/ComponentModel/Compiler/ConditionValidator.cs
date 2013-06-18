namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel;

    public class ConditionValidator : DependencyObjectValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            if (!(obj is ActivityCondition))
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ActivityCondition).FullName }), "obj");
            }
            errors.AddRange(this.ValidateProperties(manager, obj));
            return errors;
        }
    }
}

