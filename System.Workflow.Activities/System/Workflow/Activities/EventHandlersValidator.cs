namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class EventHandlersValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            EventHandlersActivity activity = obj as EventHandlersActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(EventHandlersActivity).FullName }), "obj");
            }
            if (activity.Parent == null)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MustHaveParent"), 0x522));
                return errors;
            }
            if (!(activity.Parent is EventHandlingScopeActivity))
            {
                errors.Add(new ValidationError(SR.GetString("Error_EventHandlersDeclParentNotScope", new object[] { activity.Parent.QualifiedName }), 0x522));
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

