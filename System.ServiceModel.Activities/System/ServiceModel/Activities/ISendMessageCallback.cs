namespace System.ServiceModel.Activities
{
    using System;
    using System.ServiceModel;

    public interface ISendMessageCallback
    {
        void OnSendMessage(OperationContext operationContext);
    }
}

