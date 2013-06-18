namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    internal class StateMachineSubscriptionManager
    {
        private List<StateMachineSubscription> _eventQueue = new List<StateMachineSubscription>();
        private StateMachineExecutionState _executionState;
        private System.Workflow.Activities.SetStateSubscription _setStateSubscription;
        private Dictionary<IComparable, StateMachineSubscription> _subscriptions = new Dictionary<IComparable, StateMachineSubscription>();

        internal StateMachineSubscriptionManager(StateMachineExecutionState executionState, Guid instanceId)
        {
            this._executionState = executionState;
            this._setStateSubscription = new System.Workflow.Activities.SetStateSubscription(instanceId);
        }

        internal static void ChangeEventDrivenQueueState(ActivityExecutionContext context, EventDrivenActivity eventDriven, bool enabled)
        {
            IComparable queueName = GetQueueName(StateMachineHelpers.GetEventActivity(eventDriven));
            if (queueName != null)
            {
                WorkflowQueue workflowQueue = GetWorkflowQueue(context, queueName);
                if (workflowQueue != null)
                {
                    workflowQueue.Enabled = enabled;
                }
            }
        }

        private static void ChangeStateWorkflowQueuesState(ActivityExecutionContext context, StateActivity state, bool enabled)
        {
            foreach (Activity activity in state.EnabledActivities)
            {
                EventDrivenActivity eventDriven = activity as EventDrivenActivity;
                if (eventDriven != null)
                {
                    ChangeEventDrivenQueueState(context, eventDriven, enabled);
                }
            }
        }

        internal void CreateSetStateEventQueue(ActivityExecutionContext context)
        {
            this.SetStateSubscription.CreateQueue(context);
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = this.SetStateSubscription;
        }

        internal void DeleteSetStateEventQueue(ActivityExecutionContext context)
        {
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = null;
            this.SetStateSubscription.DeleteQueue(context);
        }

        internal StateMachineSubscription Dequeue()
        {
            StateMachineSubscription subscription = this.EventQueue[0];
            this.EventQueue.RemoveAt(0);
            return subscription;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static void DisableStateWorkflowQueues(ActivityExecutionContext context, StateActivity state)
        {
            ChangeStateWorkflowQueuesState(context, state, false);
        }

        internal void Enqueue(ActivityExecutionContext context, Guid subscriptionId)
        {
            StateMachineSubscription item = this.GetSubscription(subscriptionId);
            if (item != null)
            {
                this.EventQueue.Add(item);
            }
            this.ProcessQueue(context);
        }

        internal void Enqueue(ActivityExecutionContext context, IComparable queueName)
        {
            StateMachineSubscription item = this.GetSubscription(queueName);
            if (item != null)
            {
                this.EventQueue.Add(item);
            }
            this.ProcessQueue(context);
        }

        private static IComparable GetQueueName(IEventActivity eventActivity)
        {
            return eventActivity.QueueName;
        }

        private StateMachineSubscription GetSubscription(IComparable queueName)
        {
            StateMachineSubscription subscription;
            this.Subscriptions.TryGetValue(queueName, out subscription);
            return subscription;
        }

        private EventActivitySubscription GetSubscription(IEventActivity eventActivity)
        {
            IComparable queueName = GetQueueName(eventActivity);
            if ((queueName != null) && this.Subscriptions.ContainsKey(queueName))
            {
                EventActivitySubscription subscription = this.Subscriptions[queueName] as EventActivitySubscription;
                Activity activity = (Activity) eventActivity;
                if ((subscription != null) && !(subscription.EventActivityName != activity.QualifiedName))
                {
                    return subscription;
                }
            }
            return null;
        }

        private Dictionary<IComparable, StateMachineSubscription> GetSubscriptionsShallowCopy()
        {
            Dictionary<IComparable, StateMachineSubscription> dictionary = new Dictionary<IComparable, StateMachineSubscription>();
            foreach (KeyValuePair<IComparable, StateMachineSubscription> pair in this.Subscriptions)
            {
                dictionary.Add(pair.Key, pair.Value);
            }
            return dictionary;
        }

        internal static WorkflowQueue GetWorkflowQueue(ActivityExecutionContext context, IComparable queueName)
        {
            WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
            if (service.Exists(queueName))
            {
                return service.GetWorkflowQueue(queueName);
            }
            return null;
        }

        private bool IsEventDrivenSubscribed(EventDrivenActivity eventDriven)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            return (this.GetSubscription(eventActivity) != null);
        }

        private bool IsParentState(StateActivity state, string stateName)
        {
            for (StateActivity activity = state.Parent as StateActivity; activity != null; activity = activity.Parent as StateActivity)
            {
                if (activity.QualifiedName.Equals(stateName))
                {
                    return true;
                }
            }
            return false;
        }

        internal void ProcessQueue(ActivityExecutionContext context)
        {
            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            if (((this.EventQueue.Count != 0) && !this.ExecutionState.HasEnqueuedActions) && (!this.ExecutionState.SchedulerBusy && (currentState != null)))
            {
                this.Dequeue().ProcessEvent(context);
            }
        }

        internal void ReevaluateSubscriptions(ActivityExecutionContext context)
        {
            Dictionary<IComparable, StateMachineSubscription> subscriptionsShallowCopy = this.GetSubscriptionsShallowCopy();
            List<IComparable> list = new List<IComparable>();
            for (StateActivity activity = StateMachineHelpers.GetCurrentState(context); activity != null; activity = activity.Parent as StateActivity)
            {
                foreach (Activity activity2 in activity.EnabledActivities)
                {
                    EventDrivenActivity eventDriven = activity2 as EventDrivenActivity;
                    if (eventDriven != null)
                    {
                        IComparable queueName = StateMachineHelpers.GetEventActivity(eventDriven).QueueName;
                        if (queueName != null)
                        {
                            StateMachineSubscription subscription;
                            subscriptionsShallowCopy.TryGetValue(queueName, out subscription);
                            EventActivitySubscription subscription2 = subscription as EventActivitySubscription;
                            if (subscription2 != null)
                            {
                                if (subscription2.EventDrivenName.Equals(eventDriven.QualifiedName))
                                {
                                    list.Add(queueName);
                                    continue;
                                }
                                if (subscription2.StateName.Equals(activity.QualifiedName))
                                {
                                    throw new InvalidOperationException(SR.GetStateAlreadySubscribesToThisEvent(activity.QualifiedName, queueName));
                                }
                                if (this.IsParentState(activity, subscription2.StateName))
                                {
                                    UnsubscribeAction action = new UnsubscribeAction(subscription2.StateName, subscription2.EventDrivenName);
                                    this.ExecutionState.EnqueueAction(action);
                                    subscriptionsShallowCopy.Remove(queueName);
                                }
                            }
                            if (!list.Contains(queueName))
                            {
                                SubscribeAction action2 = new SubscribeAction(activity.QualifiedName, eventDriven.QualifiedName);
                                this.ExecutionState.EnqueueAction(action2);
                                list.Add(queueName);
                            }
                        }
                    }
                }
            }
            DisableQueuesAction action3 = new DisableQueuesAction(StateMachineHelpers.GetCurrentState(context).QualifiedName);
            this.ExecutionState.EnqueueAction(action3);
        }

        private void RemoveFromQueue(Guid subscriptionId)
        {
            this.EventQueue.RemoveAll(subscription => subscription.SubscriptionId.Equals(subscriptionId));
        }

        private StateMachineSubscription SubscribeEventActivity(ActivityExecutionContext context, IEventActivity eventActivity)
        {
            EventActivitySubscription subscription = new EventActivitySubscription();
            StateActivity state = (StateActivity) context.Activity;
            subscription.Subscribe(context, state, eventActivity);
            WorkflowQueue workflowQueue = GetWorkflowQueue(context, subscription.QueueName);
            if (workflowQueue != null)
            {
                workflowQueue.Enabled = true;
            }
            this.Subscriptions[subscription.QueueName] = subscription;
            return subscription;
        }

        internal void SubscribeEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            Activity activity1 = (Activity) eventActivity;
            GetQueueName(eventActivity);
            this.SubscribeEventActivity(context, eventActivity);
        }

        internal void SubscribeToSetStateEvent(ActivityExecutionContext context)
        {
            this.SetStateSubscription.Subscribe(context);
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = this.SetStateSubscription;
        }

        private void UnsubscribeEventActivity(ActivityExecutionContext context, IEventActivity eventActivity)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (eventActivity == null)
            {
                throw new ArgumentNullException("eventActivity");
            }
            EventActivitySubscription subscription = this.GetSubscription(eventActivity);
            WorkflowQueue workflowQueue = GetWorkflowQueue(context, subscription.QueueName);
            if (workflowQueue != null)
            {
                workflowQueue.Enabled = false;
            }
            this.UnsubscribeEventActivity(context, eventActivity, subscription);
        }

        private void UnsubscribeEventActivity(ActivityExecutionContext context, IEventActivity eventActivity, EventActivitySubscription subscription)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (eventActivity == null)
            {
                throw new ArgumentNullException("eventActivity");
            }
            if (subscription == null)
            {
                throw new ArgumentNullException("subscription");
            }
            subscription.Unsubscribe(context, eventActivity);
            this.RemoveFromQueue(subscription.SubscriptionId);
            this.Subscriptions.Remove(subscription.QueueName);
        }

        internal void UnsubscribeEventDriven(ActivityExecutionContext context, EventDrivenActivity eventDriven)
        {
            IEventActivity eventActivity = StateMachineHelpers.GetEventActivity(eventDriven);
            this.UnsubscribeEventActivity(context, eventActivity);
        }

        internal void UnsubscribeState(ActivityExecutionContext context)
        {
            StateActivity activity = (StateActivity) context.Activity;
            foreach (Activity activity2 in activity.EnabledActivities)
            {
                EventDrivenActivity eventDriven = activity2 as EventDrivenActivity;
                if ((eventDriven != null) && this.IsEventDrivenSubscribed(eventDriven))
                {
                    this.UnsubscribeEventDriven(context, eventDriven);
                }
            }
        }

        internal void UnsubscribeToSetStateEvent(ActivityExecutionContext context)
        {
            this.Subscriptions[this.SetStateSubscription.SubscriptionId] = null;
            this.SetStateSubscription.Unsubscribe(context);
        }

        private List<StateMachineSubscription> EventQueue
        {
            get
            {
                return this._eventQueue;
            }
        }

        internal StateMachineExecutionState ExecutionState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._executionState;
            }
        }

        internal System.Workflow.Activities.SetStateSubscription SetStateSubscription
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._setStateSubscription;
            }
        }

        internal Dictionary<IComparable, StateMachineSubscription> Subscriptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscriptions;
            }
        }
    }
}

