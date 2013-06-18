namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class TwoPhaseCommitParticipantDispatcher11 : IWSTwoPhaseCommitParticipant11, IWSTwoPhaseCommitParticipant
    {
        private TwoPhaseCommitParticipantDispatcher twoPhaseCommitParticipantDispatcher;

        public TwoPhaseCommitParticipantDispatcher11(CoordinationService service, ITwoPhaseCommitParticipant dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion11(service.ProtocolVersion, typeof(TwoPhaseCommitParticipantDispatcher11), "constr");
            this.twoPhaseCommitParticipantDispatcher = new TwoPhaseCommitParticipantDispatcher(service, dispatch);
        }

        public void Commit(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.Commit(message);
        }

        public void Prepare(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.Prepare(message);
        }

        public void Rollback(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.Rollback(message);
        }

        public void WsaFault(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.WsaFault(message);
        }

        public void WsatFault(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.WsatFault(message);
        }

        public void WscoorFault(Message message)
        {
            this.twoPhaseCommitParticipantDispatcher.WscoorFault(message);
        }

        public System.Type ContractType
        {
            get
            {
                return typeof(IWSTwoPhaseCommitParticipant11);
            }
        }
    }
}

