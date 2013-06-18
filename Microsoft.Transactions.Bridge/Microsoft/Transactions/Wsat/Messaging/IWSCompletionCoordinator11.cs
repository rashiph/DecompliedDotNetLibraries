namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="CompletionCoordinatorPortType", Namespace="http://docs.oasis-open.org/ws-tx/wsat/2006/06")]
    internal interface IWSCompletionCoordinator11 : IWSCompletionCoordinator
    {
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Commit", IsOneWay=true)]
        void Commit(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Rollback", IsOneWay=true)]
        void Rollback(Message message);
        [OperationContract(Action="http://www.w3.org/2005/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

