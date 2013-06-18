namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract]
    internal interface IDatagramService : IDisposable
    {
        [OperationContract(Action="*", IsOneWay=true, AsyncPattern=true)]
        IAsyncResult BeginSend(Message message, AsyncCallback callback, object state);
        void EndSend(IAsyncResult ar);
    }
}

