namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="RegistrationCoordinatorPortType", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wscoor")]
    internal interface IWSRegistrationCoordinator10 : IWSRegistrationCoordinator
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wscoor/Register", ReplyAction="http://schemas.xmlsoap.org/ws/2004/10/wscoor/RegisterResponse", IsOneWay=false, AsyncPattern=true)]
        IAsyncResult BeginRegister(Message message, AsyncCallback callback, object state);
        Message EndRegister(IAsyncResult ar);
    }
}

