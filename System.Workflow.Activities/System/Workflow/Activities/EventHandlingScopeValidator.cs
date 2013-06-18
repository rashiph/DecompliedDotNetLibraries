namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal sealed class EventHandlingScopeValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            EventHandlingScopeActivity activity = obj as EventHandlingScopeActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(EventHandlingScopeActivity).FullName }), "obj");
            }
            int num = 0;
            int num2 = 0;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                if (activity2 is EventHandlersActivity)
                {
                    num2++;
                }
                else
                {
                    num++;
                }
            }
            if (num > 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MoreThanTwoActivitiesInEventHandlingScope", new object[] { activity.QualifiedName }), 0x61e));
            }
            if (num2 > 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MoreThanOneEventHandlersDecl", new object[] { activity.GetType().Name }), 0x527));
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
            if (((activity.ExecutionStatus != ActivityExecutionStatus.Initialized) && (activity.ExecutionStatus != ActivityExecutionStatus.Executing)) && (activity.ExecutionStatus != ActivityExecutionStatus.Closed))
            {
                return new ValidationError(SR.GetString("Error_DynamicActivity2", new object[] { activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus), activity.GetType().FullName }), 0x50f);
            }
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (action is AddedActivityAction))
            {
                return new ValidationError(SR.GetString("Error_DynamicActivity3", new object[] { activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus), activity.GetType().FullName }), 0x50f);
            }
            return null;
        }
    }
}

