namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;

    internal sealed class SynchronizationValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            Activity activity = obj as Activity;
            if (activity != null)
            {
                ICollection<string> is2 = activity.GetValue(Activity.SynchronizationHandlesProperty) as ICollection<string>;
                if (is2 == null)
                {
                    return errors;
                }
                foreach (string str in is2)
                {
                    ValidationError item = ValidationHelpers.ValidateIdentifier("SynchronizationHandles", manager, str);
                    if (item != null)
                    {
                        errors.Add(item);
                    }
                }
            }
            return errors;
        }
    }
}

