namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    internal abstract class StateMachineSubscription : IActivityEventListener<QueueEventArgs>
    {
        private Guid _subscriptionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected StateMachineSubscription()
        {
        }

        protected abstract void Enqueue(ActivityExecutionContext context);
        internal abstract void ProcessEvent(ActivityExecutionContext context);
        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            this.Enqueue(context);
        }

        internal Guid SubscriptionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._subscriptionId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._subscriptionId = value;
            }
        }
    }
}

