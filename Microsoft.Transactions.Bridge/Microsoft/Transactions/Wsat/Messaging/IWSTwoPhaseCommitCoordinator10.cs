namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="CoordinatorPortType", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wsat")]
    internal interface IWSTwoPhaseCommitCoordinator10 : IWSTwoPhaseCommitCoordinator
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted", IsOneWay=true)]
        void Aborted(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed", IsOneWay=true)]
        void Committed(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepared", IsOneWay=true)]
        void Prepared(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/ReadOnly", IsOneWay=true)]
        void ReadOnly(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Replay", IsOneWay=true)]
        void Replay(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

