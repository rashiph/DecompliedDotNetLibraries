namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class EventDrivenValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            EventDrivenActivity activity = obj as EventDrivenActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(EventDrivenActivity).FullName }), "obj");
            }
            if ((!(activity.Parent is ListenActivity) && !(activity.Parent is EventHandlersActivity)) && !(activity.Parent is StateActivity))
            {
                errors.Add(new ValidationError(SR.GetError_EventDrivenParentNotListen(), 0x510));
            }
            string errorText = string.Empty;
            int errorNumber = -1;
            Activity activity2 = (activity.EnabledActivities.Count > 0) ? activity.EnabledActivities[0] : null;
            if (activity2 == null)
            {
                errorText = SR.GetString("Error_EventDrivenNoFirstActivity");
                errorNumber = 0x511;
            }
            else if (!(activity2 is IEventActivity))
            {
                errorText = SR.GetError_EventDrivenInvalidFirstActivity();
                errorNumber = 0x512;
            }
            if (errorText.Length > 0)
            {
                errors.Add(new ValidationError(errorText, errorNumber));
            }
            return errors;
        }

        public override ValidationError ValidateActivityChange(Activity activity, ActivityChangeAction action)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            RemovedActivityAction action2 = action as RemovedActivityAction;
            if ((action2 != null) && (action2.RemovedActivityIndex == 0))
            {
                return new ValidationError(SR.GetString("Error_EventActivityIsImmutable"), 260, false);
            }
            AddedActivityAction action3 = action as AddedActivityAction;
            if ((action3 != null) && (action3.Index == 0))
            {
                return new ValidationError(SR.GetString("Error_EventActivityIsImmutable"), 260, false);
            }
            return base.ValidateActivityChange(activity, action);
        }
    }
}

