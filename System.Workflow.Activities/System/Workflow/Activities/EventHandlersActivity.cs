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

    [ToolboxItem(false), ActivityValidator(typeof(EventHandlersValidator)), AlternateFlowActivity, Designer(typeof(EventHandlersDesigner), typeof(IDesigner)), SRCategory("Standard"), ToolboxBitmap(typeof(EventHandlersActivity), "Resources.events.png")]
    public sealed class EventHandlersActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        private static DependencyProperty ActivityStateProperty = DependencyProperty.Register("ActivityState", typeof(List<EventHandlerEventActivitySubscriber>), typeof(EventHandlersActivity));
        private static DependencyProperty IsScopeCompletedProperty = DependencyProperty.Register("IsScopeCompleted", typeof(bool), typeof(EventHandlersActivity), new PropertyMetadata(false));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventHandlersActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventHandlersActivity(string name) : base(name)
        {
        }

        private bool AllHandlersAreQuiet(EventHandlersActivity handlers, ActivityExecutionContext context)
        {
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            for (int i = 0; i < handlers.EnabledActivities.Count; i++)
            {
                EventDrivenActivity activity = handlers.EnabledActivities[i] as EventDrivenActivity;
                if ((executionContextManager.GetExecutionContext(activity) != null) || ((handlers.ActivityState != null) && (handlers.ActivityState[i].PendingExecutionCount > 0)))
                {
                    return false;
                }
            }
            return true;
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.ActivityState == null)
            {
                return ActivityExecutionStatus.Closed;
            }
            bool isScopeCompleted = this.IsScopeCompleted;
            bool flag2 = true;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                EventDrivenActivity activity = base.EnabledActivities[i] as EventDrivenActivity;
                EventHandlerEventActivitySubscriber parentEventHandler = this.ActivityState[i];
                parentEventHandler.PendingExecutionCount = 0;
                ActivityExecutionContext context = executionContext.ExecutionContextManager.GetExecutionContext(activity);
                if (context != null)
                {
                    switch (context.Activity.ExecutionStatus)
                    {
                        case ActivityExecutionStatus.Executing:
                            context.CancelActivity(context.Activity);
                            flag2 = false;
                            break;

                        case ActivityExecutionStatus.Canceling:
                        case ActivityExecutionStatus.Faulting:
                            flag2 = false;
                            break;
                    }
                }
                if (!isScopeCompleted)
                {
                    activity.EventActivity.Unsubscribe(executionContext, parentEventHandler);
                }
            }
            if (flag2)
            {
                this.ActivityState = null;
                return ActivityExecutionStatus.Closed;
            }
            return base.ExecutionStatus;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            List<EventHandlerEventActivitySubscriber> list = new List<EventHandlerEventActivitySubscriber>();
            this.ActivityState = list;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                EventDrivenActivity eventDriven = base.EnabledActivities[i] as EventDrivenActivity;
                EventHandlerEventActivitySubscriber item = new EventHandlerEventActivitySubscriber(eventDriven);
                list.Add(item);
                eventDriven.EventActivity.Subscribe(executionContext, item);
            }
            return ActivityExecutionStatus.Executing;
        }

        public Activity GetDynamicActivity(string childActivityName)
        {
            if (childActivityName == null)
            {
                throw new ArgumentNullException("childActivityName");
            }
            Activity childActivity = null;
            for (int i = 0; i < base.EnabledActivities.Count; i++)
            {
                if (base.EnabledActivities[i].QualifiedName.Equals(childActivityName))
                {
                    childActivity = base.EnabledActivities[i];
                    break;
                }
            }
            if (childActivity == null)
            {
                throw new ArgumentException(SR.GetString("Error_EventHandlersChildNotFound"), "childActivityName");
            }
            return this.GetDynamicActivity(childActivity);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private Activity GetDynamicActivity(Activity childActivity)
        {
            if (childActivity == null)
            {
                throw new ArgumentNullException("childActivity");
            }
            if (!base.EnabledActivities.Contains(childActivity))
            {
                throw new ArgumentException(SR.GetString("Error_EventHandlersChildNotFound"), "childActivity");
            }
            Activity[] dynamicActivities = base.GetDynamicActivities(childActivity);
            if (dynamicActivities.Length != 0)
            {
                return dynamicActivities[0];
            }
            return null;
        }

        protected override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        protected override void OnActivityChangeAdd(ActivityExecutionContext executionContext, Activity addedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (addedActivity == null)
            {
                throw new ArgumentNullException("addedActivity");
            }
            EventDrivenActivity eventDriven = addedActivity as EventDrivenActivity;
            EventHandlersActivity activity = (EventHandlersActivity) executionContext.Activity;
            EventHandlerEventActivitySubscriber parentEventHandler = new EventHandlerEventActivitySubscriber(eventDriven);
            if (((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (activity.ActivityState != null)) && !activity.IsScopeCompleted)
            {
                eventDriven.EventActivity.Subscribe(executionContext, parentEventHandler);
                activity.ActivityState.Insert(activity.EnabledActivities.IndexOf(addedActivity), parentEventHandler);
            }
        }

        protected override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (removedActivity == null)
            {
                throw new ArgumentNullException("removedActivity");
            }
            EventDrivenActivity activity = removedActivity as EventDrivenActivity;
            EventHandlersActivity activity2 = (EventHandlersActivity) executionContext.Activity;
            if (((activity2.ExecutionStatus == ActivityExecutionStatus.Executing) && (activity2.ActivityState != null)) && !activity2.IsScopeCompleted)
            {
                for (int i = 0; i < activity2.ActivityState.Count; i++)
                {
                    EventHandlerEventActivitySubscriber parentEventHandler = activity2.ActivityState[i];
                    if (parentEventHandler.eventDrivenActivity.QualifiedName.Equals(removedActivity.QualifiedName))
                    {
                        activity.EventActivity.Unsubscribe(executionContext, parentEventHandler);
                        activity2.ActivityState.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(ActivityStateProperty);
            base.RemoveProperty(IsScopeCompletedProperty);
        }

        private void OnUnsubscribeAndClose(object sender, EventArgs args)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (args == null)
            {
                throw new ArgumentNullException("args");
            }
            ActivityExecutionContext parentContext = (ActivityExecutionContext) sender;
            if (parentContext == null)
            {
                throw new ArgumentException("sender");
            }
            EventHandlersActivity activity = parentContext.Activity as EventHandlersActivity;
            if (parentContext.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                activity.IsScopeCompleted = true;
                ActivityExecutionContextManager executionContextManager = parentContext.ExecutionContextManager;
                bool flag = true;
                for (int i = 0; i < activity.EnabledActivities.Count; i++)
                {
                    EventDrivenActivity activity2 = activity.EnabledActivities[i] as EventDrivenActivity;
                    EventHandlerEventActivitySubscriber parentEventHandler = activity.ActivityState[i];
                    activity2.EventActivity.Unsubscribe(parentContext, parentEventHandler);
                    if ((executionContextManager.GetExecutionContext(activity2) != null) || (activity.ActivityState[i].PendingExecutionCount != 0))
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    activity.ActivityState = null;
                    parentContext.CloseActivity();
                }
            }
        }

        protected override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            base.OnWorkflowChangesCompleted(executionContext);
            if (this.ActivityState != null)
            {
                switch (base.ExecutionStatus)
                {
                    case ActivityExecutionStatus.Executing:
                        if (this.IsScopeCompleted && this.AllHandlersAreQuiet(this, executionContext))
                        {
                            executionContext.CloseActivity();
                        }
                        return;

                    case ActivityExecutionStatus.Canceling:
                    case ActivityExecutionStatus.Faulting:
                        if (this.AllHandlersAreQuiet(this, executionContext))
                        {
                            executionContext.CloseActivity();
                        }
                        return;

                    case ActivityExecutionStatus.Closed:
                    case ActivityExecutionStatus.Compensating:
                        return;
                }
            }
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
            EventDrivenActivity activity = e.Activity as EventDrivenActivity;
            EventHandlersActivity handlers = context.Activity as EventHandlersActivity;
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            executionContextManager.CompleteExecutionContext(executionContextManager.GetExecutionContext(activity));
            switch (handlers.ExecutionStatus)
            {
                case ActivityExecutionStatus.Executing:
                    for (int i = 0; i < handlers.EnabledActivities.Count; i++)
                    {
                        if (handlers.EnabledActivities[i].QualifiedName.Equals(activity.QualifiedName))
                        {
                            EventHandlerEventActivitySubscriber subscriber = handlers.ActivityState[i];
                            if (subscriber.PendingExecutionCount > 0)
                            {
                                subscriber.PendingExecutionCount--;
                                subscriber.IsBlocked = false;
                                ActivityExecutionContext context2 = executionContextManager.CreateExecutionContext(handlers.EnabledActivities[i]);
                                context2.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                                context2.ExecuteActivity(context2.Activity);
                                return;
                            }
                            subscriber.IsBlocked = true;
                            if (handlers.IsScopeCompleted && this.AllHandlersAreQuiet(handlers, context))
                            {
                                context.CloseActivity();
                                return;
                            }
                            break;
                        }
                    }
                    return;

                case ActivityExecutionStatus.Canceling:
                case ActivityExecutionStatus.Faulting:
                    if (this.AllHandlersAreQuiet(handlers, context))
                    {
                        context.CloseActivity();
                    }
                    break;

                case ActivityExecutionStatus.Closed:
                case ActivityExecutionStatus.Compensating:
                    break;

                default:
                    return;
            }
        }

        internal void UnsubscribeAndClose()
        {
            base.Invoke<EventArgs>(new EventHandler<EventArgs>(this.OnUnsubscribeAndClose), EventArgs.Empty);
        }

        private List<EventHandlerEventActivitySubscriber> ActivityState
        {
            get
            {
                return (List<EventHandlerEventActivitySubscriber>) base.GetValue(ActivityStateProperty);
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

        private bool IsScopeCompleted
        {
            get
            {
                return (bool) base.GetValue(IsScopeCompletedProperty);
            }
            set
            {
                base.SetValue(IsScopeCompletedProperty, value);
            }
        }

        [Serializable]
        private sealed class EventHandlerEventActivitySubscriber : IActivityEventListener<QueueEventArgs>
        {
            internal EventDrivenActivity eventDrivenActivity;
            private bool isBlocked = true;
            private int numOfMsgs = 0;

            internal EventHandlerEventActivitySubscriber(EventDrivenActivity eventDriven)
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
                ActivityExecutionContext context = sender as ActivityExecutionContext;
                if (context == null)
                {
                    throw new ArgumentException("sender");
                }
                EventHandlersActivity activityStatusChangeListener = context.Activity as EventHandlersActivity;
                if ((activityStatusChangeListener.ExecutionStatus == ActivityExecutionStatus.Executing) && activityStatusChangeListener.EnabledActivities.Contains(this.eventDrivenActivity))
                {
                    if (this.IsBlocked)
                    {
                        this.IsBlocked = false;
                        ActivityExecutionContext context2 = context.ExecutionContextManager.CreateExecutionContext(this.eventDrivenActivity);
                        context2.Activity.RegisterForStatusChange(Activity.ClosedEvent, activityStatusChangeListener);
                        context2.ExecuteActivity(context2.Activity);
                    }
                    else
                    {
                        this.PendingExecutionCount++;
                    }
                }
            }

            internal bool IsBlocked
            {
                get
                {
                    return this.isBlocked;
                }
                set
                {
                    this.isBlocked = value;
                }
            }

            internal int PendingExecutionCount
            {
                get
                {
                    return this.numOfMsgs;
                }
                set
                {
                    this.numOfMsgs = value;
                }
            }
        }
    }
}

