namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    public class CompositeActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            CompositeActivity activity = obj as CompositeActivity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(CompositeActivity).FullName }), "obj");
            }
            if (Helpers.IsActivityLocked(activity))
            {
                return new ValidationErrorCollection();
            }
            ValidationErrorCollection errors = base.Validate(manager, obj);
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            foreach (Activity activity2 in ((ISupportAlternateFlow) activity).AlternateFlowActivities)
            {
                num += (activity2 is CancellationHandlerActivity) ? 1 : 0;
                num2 += (activity2 is FaultHandlersActivity) ? 1 : 0;
                num3 += (activity2 is CompensationHandlerActivity) ? 1 : 0;
            }
            if (num > 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MoreThanOneCancelHandler", new object[] { activity.GetType().Name }), 0x527));
            }
            if (num2 > 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MoreThanOneFaultHandlersActivityDecl", new object[] { activity.GetType().Name }), 0x52a));
            }
            if (num3 > 1)
            {
                errors.Add(new ValidationError(SR.GetString("Error_MoreThanOneCompensationDecl", new object[] { activity.GetType().Name }), 0x52b));
            }
            if (manager.ValidateChildActivities)
            {
                foreach (Activity activity3 in Helpers.GetAllEnabledActivities(activity))
                {
                    errors.AddRange(ValidationHelpers.ValidateActivity(manager, activity3));
                }
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
                return new ValidationError(SR.GetString("Error_DynamicActivity", new object[] { activity.QualifiedName, Enum.GetName(typeof(ActivityExecutionStatus), activity.ExecutionStatus) }), 260);
            }
            return null;
        }
    }
}

