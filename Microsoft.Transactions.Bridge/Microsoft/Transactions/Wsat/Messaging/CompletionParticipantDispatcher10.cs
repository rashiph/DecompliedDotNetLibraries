namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class CompletionParticipantDispatcher10 : IWSCompletionParticipant10, IWSCompletionParticipant
    {
        private CompletionParticipantDispatcher completionParticipantDispatcher;

        public CompletionParticipantDispatcher10(CoordinationService service, ICompletionParticipant dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(service.ProtocolVersion, typeof(CompletionParticipantDispatcher10), "constr");
            this.completionParticipantDispatcher = new CompletionParticipantDispatcher(service, dispatch);
        }

        public void Aborted(Message message)
        {
            this.completionParticipantDispatcher.Aborted(message);
        }

        public void Committed(Message message)
        {
            this.completionParticipantDispatcher.Committed(message);
        }

        public void WsaFault(Message message)
        {
            this.completionParticipantDispatcher.WsaFault(message);
        }

        public void WsatFault(Message message)
        {
            this.completionParticipantDispatcher.WsatFault(message);
        }

        public void WscoorFault(Message message)
        {
            this.completionParticipantDispatcher.WscoorFault(message);
        }

        public System.Type ContractType
        {
            get
            {
                return typeof(IWSCompletionParticipant10);
            }
        }
    }
}

