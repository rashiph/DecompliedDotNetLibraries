namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal sealed class TransactionContextValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection errors = base.Validate(manager, obj);
            Activity activity = obj as Activity;
            if (activity == null)
            {
                throw new ArgumentException(SR.GetString("Error_UnexpectedArgumentType", new object[] { typeof(Activity).FullName }), "obj");
            }
            WorkflowTransactionOptions transactionOptions = TransactedContextFilter.GetTransactionOptions(activity);
            if (transactionOptions != null)
            {
                if (FaultAndCancellationHandlingFilter.GetFaultHandlers(activity) != null)
                {
                    ValidationError item = new ValidationError(SR.GetString("Error_AtomicScopeWithFaultHandlersActivityDecl", new object[] { activity.Name }), 0x52c);
                    errors.Add(item);
                }
                if (FaultAndCancellationHandlingFilter.GetCancellationHandler(activity) != null)
                {
                    ValidationError error2 = new ValidationError(SR.GetString("Error_AtomicScopeWithCancellationHandlerActivity", new object[] { activity.Name }), 0x575);
                    errors.Add(error2);
                }
                for (Activity activity4 = activity.Parent; activity4 != null; activity4 = activity4.Parent)
                {
                    if (activity4.SupportsTransaction)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_AtomicScopeNestedInNonLRT"), 0x52e));
                        break;
                    }
                }
                Queue<Activity> queue = new Queue<Activity>(Helpers.GetAllEnabledActivities((CompositeActivity) activity));
                while (queue.Count > 0)
                {
                    Activity activity5 = queue.Dequeue();
                    if (activity5.PersistOnClose)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_LRTScopeNestedInNonLRT"), 0x52f));
                        break;
                    }
                    if (activity5 is ICompensatableActivity)
                    {
                        errors.Add(new ValidationError(SR.GetString("Error_NestedCompensatableActivity", new object[] { activity5.QualifiedName }), 0x1a6));
                        break;
                    }
                    if (activity5 is CompositeActivity)
                    {
                        foreach (Activity activity6 in Helpers.GetAllEnabledActivities((CompositeActivity) activity5))
                        {
                            queue.Enqueue(activity6);
                        }
                    }
                }
                if (transactionOptions.TimeoutDuration.Ticks < 0L)
                {
                    ValidationError error3 = new ValidationError(SR.GetString("Error_NegativeValue", new object[] { transactionOptions.TimeoutDuration.ToString(), "TimeoutDuration" }), 0x531) {
                        PropertyName = "TimeoutDuration"
                    };
                    errors.Add(error3);
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
            AddedActivityAction action2 = action as AddedActivityAction;
            if (action2 != null)
            {
                Queue<Activity> queue = new Queue<Activity>();
                queue.Enqueue(action2.AddedActivity);
                while (queue.Count != 0)
                {
                    Activity activity2 = queue.Dequeue();
                    if (activity2.SupportsTransaction)
                    {
                        return new ValidationError(SR.GetString("Error_AtomicScopeNestedInNonLRT"), 0x52e);
                    }
                    if (activity2.PersistOnClose)
                    {
                        return new ValidationError(SR.GetString("Error_NestedPersistOnClose", new object[] { activity.QualifiedName }), 0x1a2);
                    }
                    if (activity2 is ICompensatableActivity)
                    {
                        return new ValidationError(SR.GetString("Error_NestedCompensatableActivity", new object[] { activity.QualifiedName }), 0x1a6);
                    }
                    CompositeActivity activity3 = activity2 as CompositeActivity;
                    if (activity3 != null)
                    {
                        foreach (Activity activity4 in activity3.EnabledActivities)
                        {
                            queue.Enqueue(activity4);
                        }
                    }
                }
            }
            return base.ValidateActivityChange(activity, action);
        }
    }
}

