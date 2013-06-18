namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal class EventActivitySubscription : StateMachineSubscription
    {
        private string _eventActivityName = string.Empty;
        private string _eventDrivenName = string.Empty;
        private IComparable _queueName;
        private string _stateName = string.Empty;

        protected override void Enqueue(ActivityExecutionContext context)
        {
            StateMachineExecutionState.Get(StateMachineHelpers.GetRootState((StateActivity) context.Activity)).SubscriptionManager.Enqueue(context, this.QueueName);
        }

        internal override void ProcessEvent(ActivityExecutionContext context)
        {
            StateMachineExecutionState state = StateMachineExecutionState.Get(StateMachineHelpers.GetRootState((StateActivity) context.Activity));
            ExternalEventAction action = new ExternalEventAction(this.StateName, this.EventDrivenName);
            state.EnqueueAction(action);
            state.ProcessActions(context);
        }

        internal void Subscribe(ActivityExecutionContext context, StateActivity state, IEventActivity eventActivity)
        {
            eventActivity.Subscribe(context, this);
            Activity activity = (Activity) eventActivity;
            this._queueName = eventActivity.QueueName;
            this._eventActivityName = activity.QualifiedName;
            this._stateName = state.QualifiedName;
            base.SubscriptionId = Guid.NewGuid();
            EventDrivenActivity parentEventDriven = StateMachineHelpers.GetParentEventDriven(eventActivity);
            this._eventDrivenName = parentEventDriven.QualifiedName;
        }

        internal void Unsubscribe(ActivityExecutionContext context, IEventActivity eventActivity)
        {
            eventActivity.Unsubscribe(context, this);
        }

        internal string EventActivityName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventActivityName;
            }
        }

        internal string EventDrivenName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._eventDrivenName;
            }
        }

        internal IComparable QueueName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._queueName;
            }
        }

        internal string StateName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._stateName;
            }
        }
    }
}

