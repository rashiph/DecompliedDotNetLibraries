namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class ListenValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = new ValidationErrorCollection(base.Validate(manager, obj));
            ListenActivity activity = obj as ListenActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(ListenActivity).FullName }), "obj");
            }
            if (activity.EnabledActivities.Count < 2)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ListenLessThanTwoChildren"), 0x513));
            }
            bool flag = false;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                if (!(activity2 is EventDrivenActivity))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                errors.Add(new ValidationError(SR.GetString("Error_ListenNotAllEventDriven"), 0x514));
            }
            return errors;
        }
    }
}

