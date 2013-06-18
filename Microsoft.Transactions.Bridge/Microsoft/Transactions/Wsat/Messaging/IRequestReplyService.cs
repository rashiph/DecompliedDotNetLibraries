namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract]
    internal interface IRequestReplyService : IDisposable
    {
        [OperationContract(Action="*", ReplyAction="*", IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state);
        Message EndRequest(IAsyncResult ar);
        [OperationContract(Action="*", ReplyAction="*", IsOneWay=false)]
        Message SendRequest(Message message);
    }
}

