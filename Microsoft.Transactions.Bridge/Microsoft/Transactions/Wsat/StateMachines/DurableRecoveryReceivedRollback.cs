namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableRecoveryReceivedRollback : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableRecoveryReceivedRollback(ProtocolState state) : base(state)
        {
        }

        public override void Enter(StateMachine stateMachine)
        {
            ParticipantEnlistment participant = (ParticipantEnlistment) stateMachine.Enlistment;
            participant.CreateCoordinatorService();
            base.state.TwoPhaseCommitCoordinator.SendRollback(participant);
            base.state.TransactionManagerSend.Aborted(participant);
            stateMachine.ChangeState(base.state.States.DurableAborted);
        }
    }
}

