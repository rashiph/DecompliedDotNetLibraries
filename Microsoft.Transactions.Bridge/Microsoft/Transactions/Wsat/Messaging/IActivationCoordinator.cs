namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface IActivationCoordinator
    {
        void CreateCoordinationContext(Message message, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result);
    }
}

