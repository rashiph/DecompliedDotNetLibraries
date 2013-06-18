namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="ParticipantPortType", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wsat")]
    internal interface IWSTwoPhaseCommitParticipant10 : IWSTwoPhaseCommitParticipant
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Commit", IsOneWay=true)]
        void Commit(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepare", IsOneWay=true)]
        void Prepare(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Rollback", IsOneWay=true)]
        void Rollback(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

