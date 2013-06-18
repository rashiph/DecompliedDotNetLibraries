namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    internal class SetStateSubscription : StateMachineSubscription
    {
        private Guid _instanceId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SetStateSubscription(Guid instanceId)
        {
            this._instanceId = instanceId;
        }

        internal void CreateQueue(ActivityExecutionContext context)
        {
            if (StateMachineHelpers.IsRootExecutionContext(context))
            {
                WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
                MessageEventSubscription subscription = new MessageEventSubscription("SetStateQueue", this._instanceId);
                service.CreateWorkflowQueue("SetStateQueue", true);
                base.SubscriptionId = subscription.SubscriptionId;
            }
        }

        internal void DeleteQueue(ActivityExecutionContext context)
        {
            if (StateMachineHelpers.IsRootExecutionContext(context))
            {
                WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
                service.GetWorkflowQueue("SetStateQueue");
                service.DeleteWorkflowQueue("SetStateQueue");
            }
        }

        protected override void Enqueue(ActivityExecutionContext context)
        {
            StateMachineExecutionState.Get(StateMachineHelpers.GetRootState((StateActivity) context.Activity)).SubscriptionManager.Enqueue(context, base.SubscriptionId);
        }

        internal override void ProcessEvent(ActivityExecutionContext context)
        {
            SetStateEventArgs args = context.GetService<WorkflowQueuingService>().GetWorkflowQueue("SetStateQueue").Dequeue() as SetStateEventArgs;
            StateActivity currentState = StateMachineHelpers.GetCurrentState(context);
            if (currentState == null)
            {
                throw new InvalidOperationException(SR.GetStateMachineWorkflowMustHaveACurrentState());
            }
            StateMachineExecutionState state = StateMachineExecutionState.Get(StateMachineHelpers.GetRootState((StateActivity) context.Activity));
            SetStateAction action = new SetStateAction(currentState.QualifiedName, args.TargetStateName);
            state.EnqueueAction(action);
            state.ProcessActions(context);
        }

        internal void Subscribe(ActivityExecutionContext context)
        {
            context.GetService<WorkflowQueuingService>().GetWorkflowQueue("SetStateQueue").RegisterForQueueItemAvailable(this);
        }

        internal void Unsubscribe(ActivityExecutionContext context)
        {
            context.GetService<WorkflowQueuingService>().GetWorkflowQueue("SetStateQueue").UnregisterForQueueItemAvailable(this);
        }
    }
}

