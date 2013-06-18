namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.ServiceModel;

    public interface IReceiveMessageCallback
    {
        void OnReceiveMessage(OperationContext operationContext, ExecutionProperties activityExecutionProperties);
    }
}

