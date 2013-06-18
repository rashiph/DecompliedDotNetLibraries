namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="RegistrationCoordinatorPortType", Namespace="http://docs.oasis-open.org/ws-tx/wscoor/2006/06")]
    internal interface IWSRegistrationCoordinator11 : IWSRegistrationCoordinator
    {
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/Register", ReplyAction="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/RegisterResponse", IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginRegister(Message message, AsyncCallback callback, object state);
        Message EndRegister(IAsyncResult ar);
    }
}

