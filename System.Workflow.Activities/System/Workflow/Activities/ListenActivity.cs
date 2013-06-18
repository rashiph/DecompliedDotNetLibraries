namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [SRCategory("Standard"), ActivityValidator(typeof(ListenValidator)), SRDescription("ListenActivityDescription"), ToolboxItem(typeof(ListenToolboxItem)), Designer(typeof(ListenDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(ListenActivity), "Resources.Listen.png")]
    public sealed class ListenActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        [NonSerialized]
        private bool activeBranchRemoved;
        private static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(List<ListenEventActivitySubscriber>), typeof(ListenActivity));
        private static DependencyProperty IsListenTrigerredProperty = DependencyProperty.Register("IsListenTrigerred", typeof(bool), typeof(ListenActivity), new PropertyMetadata(false));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ListenActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ListenActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.ActivityState != null)
            {
                try
                {
                    if (this.IsListenTrigerred)
                    {
                        for (int i = 0; i < base.EnabledActivities.Count; i++)
                        {
                            EventDrivenActivity activity = base.EnabledActivities[i] as EventDrivenActivity;
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
                            {
                                executionContext.CancelActivity(activity);
                                return ActivityExecutionStatus.Canceling;
                            }
                            if (activity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                            {
                                return ActivityExecutionStatus.Canceling;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < this.ActivityState.Count; j++)
                        {
                            EventDrivenActivity activity2 = base.EnabledActivities[j] as EventDrivenActivity;
                            ListenEventActivitySubscriber parentEventHandler = this.ActivityState[j];
                            activity2.EventActivity.Unsubscribe(executionContext, parentEventHandler);
                        }
                    }
                }
                finally
                {
                    this.ActivityState = null;
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            List<ListenEventActivitySubscriber> list = new List<ListenEventActivitySubscriber>();
            this.ActivityState = list;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                EventDrivenActivity eventDriven = base.EnabledActivities[i] as EventDrivenActivity;
                ListenEventActivitySubscriber parentEventHandler = new ListenEventActivitySubscriber(eventDriven);
                eventDriven.EventActivity.Subscribe(executionContext, parentEventHandler);
                list.Add(parentEventHandler);
            }
            return ActivityExecutionStatus.Executing;
        }

        protected sealed override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            ListenActivity activity = executionContext.Activity as ListenActivity;
            if (((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (activity.ActivityState != null)) && !activity.IsListenTrigerred)
            {
                EventDrivenActivity eventDriven = addedActivity as EventDrivenActivity;
                ListenEventActivitySubscriber parentEventHandler = new ListenEventActivitySubscriber(eventDriven);
                eventDriven.EventActivity.Subscribe(executionContext, parentEventHandler);
                activity.ActivityState.Insert(activity.EnabledActivities.IndexOf(addedActivity), parentEventHandler);
            }
        }

        protected sealed override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            ListenActivity activity = executionContext.Activity as ListenActivity;
            if (((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (activity.ActivityState != null)) && !activity.IsListenTrigerred)
            {
                EventDrivenActivity activity2 = removedActivity as EventDrivenActivity;
                for (int i = 0; i < activity.ActivityState.Count; i++)
                {
                    ListenEventActivitySubscriber parentEventHandler = activity.ActivityState[i];
                    if (parentEventHandler.eventDrivenActivity.QualifiedName.Equals(activity2.QualifiedName))
                    {
                        activity2.EventActivity.Unsubscribe(executionContext, parentEventHandler);
                        activity.ActivityState.RemoveAt(i);
                        return;
                    }
                }
            }
            else if (this.IsListenTrigerred && (removedActivity.ExecutionStatus == ActivityExecutionStatus.Closed))
            {
                this.activeBranchRemoved = true;
            }
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(IsListenTrigerredProperty);
            base.RemoveProperty(ActivityStateProperty);
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            base.OnWorkflowChangesCompleted(executionContext);
            if (this.activeBranchRemoved)
            {
                executionContext.CloseActivity();
            }
            this.activeBranchRemoved = false;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            context.CloseActivity();
        }

        private List<ListenEventActivitySubscriber> ActivityState
        {
            get
            {
                return (List<ListenEventActivitySubscriber>) base.GetValue(ActivityStateProperty);
            }
            set
            {
                if (value == null)
                {
                    base.RemoveProperty(ActivityStateProperty);
                }
                else
                {
                    base.SetValue(ActivityStateProperty, value);
                }
            }
        }

        private bool IsListenTrigerred
        {
            get
            {
                return (bool) base.GetValue(IsListenTrigerredProperty);
            }
            set
            {
                base.SetValue(IsListenTrigerredProperty, value);
            }
        }

        [Serializable]
        private sealed class ListenEventActivitySubscriber : IActivityEventListener<QueueEventArgs>
        {
            internal EventDrivenActivity eventDrivenActivity;

            internal ListenEventActivitySubscriber(EventDrivenActivity eventDriven)
            {
                this.eventDrivenActivity = eventDriven;
            }

            void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
            {
                if (sender == null)
                {
                    throw new ArgumentNullException("sender");
                }
                if (e == null)
                {
                    throw new ArgumentNullException("e");
                }
                ActivityExecutionContext parentContext = sender as ActivityExecutionContext;
                if (parentContext == null)
                {
                    throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
                }
                ListenActivity activityStatusChangeListener = parentContext.Activity as ListenActivity;
                if (((!activityStatusChangeListener.IsListenTrigerred && (activityStatusChangeListener.ExecutionStatus != ActivityExecutionStatus.Canceling)) && (activityStatusChangeListener.ExecutionStatus != ActivityExecutionStatus.Closed)) && activityStatusChangeListener.EnabledActivities.Contains(this.eventDrivenActivity))
                {
                    activityStatusChangeListener.IsListenTrigerred = true;
                    for (int i = 0; i < activityStatusChangeListener.EnabledActivities.Count; i++)
                    {
                        EventDrivenActivity activity2 = activityStatusChangeListener.EnabledActivities[i] as EventDrivenActivity;
                        ListenActivity.ListenEventActivitySubscriber parentEventHandler = activityStatusChangeListener.ActivityState[i];
                        activity2.EventActivity.Unsubscribe(parentContext, parentEventHandler);
                    }
                    this.eventDrivenActivity.RegisterForStatusChange(Activity.ClosedEvent, activityStatusChangeListener);
                    parentContext.ExecuteActivity(this.eventDrivenActivity);
                }
            }
        }
    }
}

