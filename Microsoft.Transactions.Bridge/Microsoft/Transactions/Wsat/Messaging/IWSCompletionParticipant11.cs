namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="CompletionParticipantPortType", Namespace="http://docs.oasis-open.org/ws-tx/wsat/2006/06")]
    internal interface IWSCompletionParticipant11 : IWSCompletionParticipant
    {
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Aborted", IsOneWay=true)]
        void Aborted(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/Committed", IsOneWay=true)]
        void Committed(Message message);
        [OperationContract(Action="http://www.w3.org/2005/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://docs.oasis-open.org/ws-tx/wscoor/2006/06/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

