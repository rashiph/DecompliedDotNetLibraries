namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    public class ActivityValidator : DependencyObjectValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            Activity context = obj as Activity;
            if (context == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            if (manager.Context == null)
            {
                throw new ArgumentException("manager", SR.GetString("Error_MissingContextProperty"));
            }
            manager.Context.Push(context);
            ValidationErrorCollection errors = new ValidationErrorCollection();
            errors.AddRange(base.Validate(manager, obj));
            if (context.Parent == null)
            {
                errors.AddRange(ValidationHelpers.ValidateUniqueIdentifiers(context));
                if (!context.Enabled)
                {
                    ValidationError item = new ValidationError(SR.GetString("Error_RootIsNotEnabled"), 0x628) {
                        PropertyName = "Enabled"
                    };
                    errors.Add(item);
                }
            }
            Activity rootActivity = Helpers.GetRootActivity(context);
            if (context != rootActivity)
            {
                ValidationError error2 = ValidationHelpers.ValidateNameProperty("Name", manager, context.Name);
                if (error2 != null)
                {
                    errors.Add(error2);
                }
            }
            try
            {
                errors.AddRange(this.ValidateProperties(manager, obj));
            }
            finally
            {
                manager.Context.Pop();
            }
            return errors;
        }
    }
}

