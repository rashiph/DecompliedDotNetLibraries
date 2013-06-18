namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="CoordinatorPortType", Namespace="http://docs.oasis-open.org/ws-tx/wsat/2006/06")]
    internal interface IWSTwoPhaseCommitCoordinator11 : IWSTwoPhaseCommitCoordinator
    {
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Aborted", IsOneWay=true)]
        void Aborted(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Committed", IsOneWay=true)]
        void Committed(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepared", IsOneWay=true)]
        void Prepared(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/ReadOnly", IsOneWay=true)]
        void ReadOnly(Message message);
        [OperationContract(Action="http://www.w3.org/2005/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

