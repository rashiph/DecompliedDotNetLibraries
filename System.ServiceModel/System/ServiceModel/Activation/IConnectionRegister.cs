namespace System.ServiceModel.Activation
{
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(SessionMode=SessionMode.Required, CallbackContract=typeof(IConnectionDuplicator))]
    internal interface IConnectionRegister
    {
        [OperationContract(IsOneWay=false, IsInitiating=true)]
        ListenerExceptionStatus Register(Version version, int pid, BaseUriWithWildcard path, int queueId, Guid token, string eventName);
        [OperationContract]
        void Unregister();
        [OperationContract]
        bool ValidateUriRoute(Uri uri, IPAddress address, int port);
    }
}

