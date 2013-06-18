namespace System.Workflow.Activities
{
    using System;
    using System.Runtime.InteropServices;
    using System.Workflow.Runtime;

    internal interface IDeliverMessage
    {
        void DeliverMessage(ExternalDataEventArgs eventArgs, IComparable queueName, object message, object workItem, IPendingWork workHandler);
        object[] PrepareEventArgsArray(object sender, ExternalDataEventArgs eventArgs, out object workItem, out IPendingWork workHandler);
    }
}

