namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Design;

    internal static class CompensationUtils
    {
        private static bool CollectCompensatableActiveContexts(ActivityExecutionContext context, Activity targetActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            foreach (ActivityExecutionContext context2 in context.ExecutionContextManager.ExecutionContexts)
            {
                if ((targetActivity.GetActivityByName(context2.Activity.QualifiedName, true) != null) && (!immediateCompensation || !IsActivityInBackWorkBranch(targetActivity, context2.Activity)))
                {
                    if ((context2.Activity is ICompensatableActivity) && (((context2.Activity.ExecutionStatus == ActivityExecutionStatus.Compensating) || (context2.Activity.ExecutionStatus == ActivityExecutionStatus.Faulting)) || (context2.Activity.ExecutionStatus == ActivityExecutionStatus.Canceling)))
                    {
                        return true;
                    }
                    if (context2.Activity is CompositeActivity)
                    {
                        Activity[] compensatableChildren = GetCompensatableChildren(context2.Activity as CompositeActivity);
                        if (compensatableChildren != null)
                        {
                            int key = 0;
                            foreach (Activity activity in compensatableChildren)
                            {
                                int num2 = (int) activity.GetValue(Activity.CompletedOrderIdProperty);
                                if (key < num2)
                                {
                                    key = num2;
                                }
                            }
                            if (key != 0)
                            {
                                sortedListOfCompensatableTargets.Add(key, new CompensationInfo(context2));
                            }
                        }
                        CollectCompensatableActiveContexts(context2, targetActivity, sortedListOfCompensatableTargets, immediateCompensation);
                        CollectCompensatableCompletedContexts(context2, targetActivity, sortedListOfCompensatableTargets, immediateCompensation);
                    }
                }
            }
            return false;
        }

        private static void CollectCompensatableCompletedContexts(ActivityExecutionContext context, Activity targetActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            for (int i = executionContextManager.CompletedExecutionContexts.Count - 1; i >= 0; i--)
            {
                ActivityExecutionContextInfo targetExecutionInfo = executionContextManager.CompletedExecutionContexts[i];
                if (((byte) (targetExecutionInfo.Flags & PersistFlags.NeedsCompensation)) != 0)
                {
                    Activity activityByName = targetActivity.GetActivityByName(targetExecutionInfo.ActivityQualifiedName, true);
                    if ((activityByName != null) && (!immediateCompensation || !IsActivityInBackWorkBranch(targetActivity, activityByName)))
                    {
                        sortedListOfCompensatableTargets.Add(targetExecutionInfo.CompletedOrderId, new CompensationInfo(targetExecutionInfo, executionContextManager));
                    }
                }
            }
        }

        private static bool CollectCompensatableTargetActivities(CompositeActivity compositeActivity, SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets, bool immediateCompensation)
        {
            Queue<Activity> queue = new Queue<Activity>(Helpers.GetAllEnabledActivities(compositeActivity));
            while (queue.Count > 0)
            {
                Activity childActivity = queue.Dequeue();
                if (((childActivity.ExecutionStatus == ActivityExecutionStatus.Compensating) || (childActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)) || (childActivity.ExecutionStatus == ActivityExecutionStatus.Canceling))
                {
                    return true;
                }
                if (!immediateCompensation || !IsActivityInBackWorkBranch(compositeActivity, childActivity))
                {
                    if (((childActivity is ICompensatableActivity) && (childActivity.ExecutionStatus == ActivityExecutionStatus.Closed)) && (childActivity.ExecutionResult == ActivityExecutionResult.Succeeded))
                    {
                        sortedListOfCompensatableTargets.Add((int) childActivity.GetValue(Activity.CompletedOrderIdProperty), new CompensationInfo(childActivity));
                    }
                    else if (childActivity is CompositeActivity)
                    {
                        foreach (Activity activity2 in Helpers.GetAllEnabledActivities((CompositeActivity) childActivity))
                        {
                            queue.Enqueue(activity2);
                        }
                        continue;
                    }
                }
            }
            return false;
        }

        private static void CompleteRevokedExecutionContext(Activity targetActivity, ActivityExecutionContext context)
        {
            ActivityExecutionContext[] array = new ActivityExecutionContext[context.ExecutionContextManager.ExecutionContexts.Count];
            context.ExecutionContextManager.ExecutionContexts.CopyTo(array, 0);
            foreach (ActivityExecutionContext context2 in array)
            {
                if (targetActivity.GetActivityByName(context2.Activity.QualifiedName, true) != null)
                {
                    if (context2.Activity.ExecutionStatus == ActivityExecutionStatus.Closed)
                    {
                        CompleteRevokedExecutionContext(context2.Activity, context2);
                    }
                    context.ExecutionContextManager.CompleteExecutionContext(context2);
                }
            }
        }

        internal static Activity[] GetCompensatableChildren(CompositeActivity compositeActivity)
        {
            SortedDictionary<int, Activity> dictionary = new SortedDictionary<int, Activity>();
            Queue<Activity> queue = new Queue<Activity>(Helpers.GetAllEnabledActivities(compositeActivity));
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                if (((activity is ICompensatableActivity) && (activity.ExecutionStatus == ActivityExecutionStatus.Closed)) && (activity.ExecutionResult == ActivityExecutionResult.Succeeded))
                {
                    dictionary.Add((int) activity.GetValue(Activity.CompletedOrderIdProperty), activity);
                }
                else if (activity is CompositeActivity)
                {
                    foreach (Activity activity2 in Helpers.GetAllEnabledActivities((CompositeActivity) activity))
                    {
                        queue.Enqueue(activity2);
                    }
                    continue;
                }
            }
            Activity[] array = new Activity[dictionary.Count];
            dictionary.Values.CopyTo(array, 0);
            return array;
        }

        internal static Activity GetLastCompensatableChild(CompositeActivity compositeActivity)
        {
            Activity[] compensatableChildren = GetCompensatableChildren(compositeActivity);
            if (((compensatableChildren != null) && (compensatableChildren.Length > 0)) && (compensatableChildren[compensatableChildren.Length - 1] != null))
            {
                return compensatableChildren[compensatableChildren.Length - 1];
            }
            return null;
        }

        private static bool IsActivityInBackWorkBranch(Activity targetParent, Activity childActivity)
        {
            Activity parent = childActivity;
            while (parent.Parent != targetParent)
            {
                parent = parent.Parent;
            }
            return Helpers.IsFrameworkActivity(parent);
        }

        internal static bool TryCompensateLastCompletedChildActivity(ActivityExecutionContext context, Activity targetActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> statusChangeHandler)
        {
            bool flag;
            try
            {
                flag = TryCompensateLastCompletedChildActivity(context, targetActivity, statusChangeHandler, true);
            }
            catch (Exception)
            {
                if (targetActivity.Parent == null)
                {
                    CompleteRevokedExecutionContext(targetActivity, context);
                }
                throw;
            }
            return flag;
        }

        private static bool TryCompensateLastCompletedChildActivity(ActivityExecutionContext context, Activity targetActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs> statusChangeHandler, bool isimmediateCompensation)
        {
            SortedDictionary<int, CompensationInfo> sortedListOfCompensatableTargets = new SortedDictionary<int, CompensationInfo>();
            if (targetActivity is CompositeActivity)
            {
                if (CollectCompensatableTargetActivities(targetActivity as CompositeActivity, sortedListOfCompensatableTargets, isimmediateCompensation))
                {
                    return true;
                }
                if (CollectCompensatableActiveContexts(context, targetActivity, sortedListOfCompensatableTargets, isimmediateCompensation))
                {
                    return true;
                }
                CollectCompensatableCompletedContexts(context, targetActivity, sortedListOfCompensatableTargets, isimmediateCompensation);
                if (sortedListOfCompensatableTargets.Count == 0)
                {
                    CompleteRevokedExecutionContext(targetActivity, context);
                    return false;
                }
                int? nullable = targetActivity.GetValue(CompensationHandlingFilter.LastCompensatedOrderIdProperty) as int?;
                int num = -1;
                CompensationInfo info = null;
                foreach (int num2 in sortedListOfCompensatableTargets.Keys)
                {
                    if (nullable.HasValue)
                    {
                        int? nullable2 = nullable;
                        int num3 = num2;
                        if ((nullable2.GetValueOrDefault() < num3) && nullable2.HasValue)
                        {
                            break;
                        }
                    }
                    info = sortedListOfCompensatableTargets[num2];
                    num = num2;
                }
                if (info == null)
                {
                    CompleteRevokedExecutionContext(targetActivity, context);
                    return false;
                }
                targetActivity.SetValue(CompensationHandlingFilter.LastCompensatedOrderIdProperty, num);
                if ((info.TargetActivity != null) && (info.TargetActivity is ICompensatableActivity))
                {
                    info.TargetActivity.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                    context.CompensateActivity(info.TargetActivity);
                    return true;
                }
                if ((info.TargetExecutionInfo != null) && (info.TargetExecutionContextManager != null))
                {
                    ActivityExecutionContext context2 = info.TargetExecutionContextManager.DiscardPersistedExecutionContext(info.TargetExecutionInfo);
                    if (context2.Activity is ICompensatableActivity)
                    {
                        context2.Activity.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                        context2.CompensateActivity(context2.Activity);
                        return true;
                    }
                    if (context2.Activity is CompositeActivity)
                    {
                        Activity lastCompensatableChild = GetLastCompensatableChild(context2.Activity as CompositeActivity);
                        if (lastCompensatableChild != null)
                        {
                            lastCompensatableChild.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                            context2.CompensateActivity(lastCompensatableChild);
                            return true;
                        }
                        return TryCompensateLastCompletedChildActivity(context2, context2.Activity, statusChangeHandler, false);
                    }
                }
                else if ((info.TargetExecutionContext != null) && (info.TargetExecutionContext.Activity is CompositeActivity))
                {
                    Activity activity = GetLastCompensatableChild(info.TargetExecutionContext.Activity as CompositeActivity);
                    if (activity != null)
                    {
                        activity.RegisterForStatusChange(Activity.StatusChangedEvent, statusChangeHandler);
                        info.TargetExecutionContext.CompensateActivity(activity);
                        return true;
                    }
                    return TryCompensateLastCompletedChildActivity(info.TargetExecutionContext, info.TargetExecutionContext.Activity, statusChangeHandler, false);
                }
            }
            return false;
        }

        private sealed class CompensationInfo
        {
            private Activity targetActivity;
            private ActivityExecutionContext targetExecutionContext;
            private ActivityExecutionContextManager targetExecutionContextManager;
            private ActivityExecutionContextInfo targetExecutionInfo;

            internal CompensationInfo(Activity targetActivity)
            {
                this.targetActivity = targetActivity;
            }

            internal CompensationInfo(ActivityExecutionContext targetExecutionContext)
            {
                this.targetExecutionContext = targetExecutionContext;
            }

            internal CompensationInfo(ActivityExecutionContextInfo targetExecutionInfo, ActivityExecutionContextManager targetExecutionContextManager)
            {
                this.targetExecutionInfo = targetExecutionInfo;
                this.targetExecutionContextManager = targetExecutionContextManager;
            }

            internal Activity TargetActivity
            {
                get
                {
                    return this.targetActivity;
                }
            }

            internal ActivityExecutionContext TargetExecutionContext
            {
                get
                {
                    return this.targetExecutionContext;
                }
            }

            internal ActivityExecutionContextManager TargetExecutionContextManager
            {
                get
                {
                    return this.targetExecutionContextManager;
                }
            }

            internal ActivityExecutionContextInfo TargetExecutionInfo
            {
                get
                {
                    return this.targetExecutionInfo;
                }
            }
        }
    }
}

