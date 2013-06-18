namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;

    internal static class SequenceHelper
    {
        private static DependencyProperty ActiveChildQualifiedNameProperty = DependencyProperty.RegisterAttached("ActiveChildQualifiedName", typeof(string), typeof(SequenceHelper));
        private static DependencyProperty ActiveChildRemovedProperty = DependencyProperty.RegisterAttached("ActiveChildRemoved", typeof(bool), typeof(SequenceHelper), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));

        public static ActivityExecutionStatus Cancel(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            for (int i = activity.EnabledActivities.Count - 1; i >= 0; i--)
            {
                Activity activity2 = activity.EnabledActivities[i];
                if (activity2.ExecutionStatus == ActivityExecutionStatus.Executing)
                {
                    executionContext.CancelActivity(activity2);
                    return activity.ExecutionStatus;
                }
                if ((activity2.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity2.ExecutionStatus == ActivityExecutionStatus.Faulting))
                {
                    return activity.ExecutionStatus;
                }
                if (activity2.ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        public static ActivityExecutionStatus Execute(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            if (activity.EnabledActivities.Count == 0)
            {
                return ActivityExecutionStatus.Closed;
            }
            activity.EnabledActivities[0].RegisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>) activity);
            executionContext.ExecuteActivity(activity.EnabledActivities[0]);
            activity.SetValue(ActiveChildQualifiedNameProperty, activity.EnabledActivities[0].QualifiedName);
            return ActivityExecutionStatus.Executing;
        }

        public static void OnActivityChangeRemove(CompositeActivity activity, ActivityExecutionContext executionContext, Activity removedActivity)
        {
            string str = activity.GetValue(ActiveChildQualifiedNameProperty) as string;
            if (removedActivity.QualifiedName.Equals(str))
            {
                activity.SetValue(ActiveChildRemovedProperty, true);
            }
        }

        public static void OnEvent(CompositeActivity activity, object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            ActivityExecutionContext executionContext = sender as ActivityExecutionContext;
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>) activity);
            if (((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)) || ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && !TryScheduleNextChild(activity, executionContext)))
            {
                activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                executionContext.CloseActivity();
            }
        }

        public static void OnWorkflowChangesCompleted(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            string str = activity.GetValue(ActiveChildQualifiedNameProperty) as string;
            bool flag = (bool) activity.GetValue(ActiveChildRemovedProperty);
            if (((str != null) && flag) && (((activity.ExecutionStatus == ActivityExecutionStatus.Canceling) || (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)) || ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && !TryScheduleNextChild(activity, executionContext))))
            {
                activity.RemoveProperty(ActiveChildQualifiedNameProperty);
                executionContext.CloseActivity();
            }
            activity.RemoveProperty(ActiveChildRemovedProperty);
        }

        private static bool TryScheduleNextChild(CompositeActivity activity, ActivityExecutionContext executionContext)
        {
            IList<Activity> enabledActivities = activity.EnabledActivities;
            int num = 0;
            for (int i = enabledActivities.Count - 1; i >= 0; i--)
            {
                if (enabledActivities[i].ExecutionStatus == ActivityExecutionStatus.Closed)
                {
                    if (i == (enabledActivities.Count - 1))
                    {
                        return false;
                    }
                    num = i + 1;
                    break;
                }
            }
            enabledActivities[num].RegisterForStatusChange(Activity.ClosedEvent, (IActivityEventListener<ActivityExecutionStatusChangedEventArgs>) activity);
            executionContext.ExecuteActivity(enabledActivities[num]);
            activity.SetValue(ActiveChildQualifiedNameProperty, enabledActivities[num].QualifiedName);
            return true;
        }
    }
}

