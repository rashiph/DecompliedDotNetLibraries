namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceContract(Name="CompletionParticipantPortType", Namespace="http://schemas.xmlsoap.org/ws/2004/10/wsat")]
    internal interface IWSCompletionParticipant10 : IWSCompletionParticipant
    {
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted", IsOneWay=true)]
        void Aborted(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed", IsOneWay=true)]
        void Committed(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/08/addressing/fault", IsOneWay=true)]
        void WsaFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wsat/fault", IsOneWay=true)]
        void WsatFault(Message message);
        [OperationContract(Action="http://schemas.xmlsoap.org/ws/2004/10/wscoor/fault", IsOneWay=true)]
        void WscoorFault(Message message);
    }
}

