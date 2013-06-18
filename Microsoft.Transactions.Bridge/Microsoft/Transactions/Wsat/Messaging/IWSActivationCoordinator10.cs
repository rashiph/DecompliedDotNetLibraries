namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="ActivationCoordinatorPortType", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wscoor")]
    internal interface IWSActivationCoordinator10 : IWSActivationCoordinator
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContext", ReplyAction="http://schemas.xmlsoap.org/ws/2004/10/wscoor/CreateCoordinationContextResponse", IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginCreateCoordinationContext(Message message, AsyncCallback callback, object state);
        Message EndCreateCoordinationContext(IAsyncResult ar);
    }
}

