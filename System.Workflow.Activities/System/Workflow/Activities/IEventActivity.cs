namespace System.Workflow.Activities
{
    using System;
    using System.Workflow.ComponentModel;

    public interface IEventActivity
    {
        void Subscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler);
        void Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler);

        IComparable QueueName { get; }
    }
}

