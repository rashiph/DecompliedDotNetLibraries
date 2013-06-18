namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="ActivationCoordinatorPortType", Namespace="http://docs.oasis-open.org/ws-tx/wscoor/2006/06")]
    internal interface IWSActivationCoordinator11 : IWSActivationCoordinator
    {
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContext", ReplyAction="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/CreateCoordinationContextResponse", IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginCreateCoordinationContext(Message message, AsyncCallback callback, object state);
        Message EndCreateCoordinationContext(IAsyncResult ar);
    }
}

