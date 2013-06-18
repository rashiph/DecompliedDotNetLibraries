namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorPreparing : ActiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorPreparing(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgDurablePrepareEvent e)
        {
        }

        public override void OnEvent(MsgDurableRollbackEvent e)
        {
            base.state.TransactionManagerSend.Rollback(e.Coordinator);
            e.StateMachine.ChangeState(base.state.States.CoordinatorAborted);
        }

        public override void OnEvent(MsgVolatilePrepareEvent e)
        {
        }

        public override void OnEvent(TmAsyncRollbackEvent e)
        {
            base.ProcessTmAsyncRollback(e);
        }

        public override void OnEvent(TmPrepareResponseEvent e)
        {
            Microsoft.Transactions.Wsat.StateMachines.State coordinatorAborted;
            CoordinatorEnlistment coordinator = e.Coordinator;
            switch (e.Status)
            {
                case Status.Aborted:
                    coordinatorAborted = base.state.States.CoordinatorAborted;
                    break;

                case Status.Prepared:
                    base.state.TwoPhaseCommitParticipant.SendPrepared(coordinator);
                    coordinatorAborted = base.state.States.CoordinatorPrepared;
                    break;

                case Status.Readonly:
                    base.state.TwoPhaseCommitParticipant.SendDurableReadOnly(coordinator);
                    coordinatorAborted = base.state.States.CoordinatorReadOnlyInDoubt;
                    break;

                default:
                    DiagnosticUtility.FailFast("Invalid status code");
                    coordinatorAborted = null;
                    break;
            }
            e.StateMachine.ChangeState(coordinatorAborted);
        }
    }
}

