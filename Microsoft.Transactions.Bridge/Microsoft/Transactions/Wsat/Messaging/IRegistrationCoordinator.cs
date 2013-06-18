namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel.Channels;

    internal interface IRegistrationCoordinator
    {
        void Register(Message message, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result);
    }
}

