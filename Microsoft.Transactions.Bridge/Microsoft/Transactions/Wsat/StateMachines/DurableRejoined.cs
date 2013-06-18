namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class DurableRejoined : DecidedState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DurableRejoined(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgAbortedEvent e)
        {
        }

        public override void OnEvent(MsgCommittedEvent e)
        {
        }

        public override void OnEvent(MsgPreparedEvent e)
        {
        }

        public override void OnEvent(MsgReplayEvent e)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(base.state.ProtocolVersion, typeof(DurableRejoined), "OnEvent(replay)");
        }

        public override void OnEvent(TmCommitEvent e)
        {
            if (base.state.Recovering && base.state.TryEnqueueRecoveryOutcome(e))
            {
                e.StateMachine.ChangeState(base.state.States.DurableRecoveryAwaitingCommit);
            }
            else
            {
                e.Participant.SetCallback(e.Callback, e.CallbackState);
                e.StateMachine.ChangeState(base.state.States.DurableRecoveryReceivedCommit);
            }
        }

        public override void OnEvent(TmRollbackEvent e)
        {
            if (base.state.Recovering && base.state.TryEnqueueRecoveryOutcome(e))
            {
                e.StateMachine.ChangeState(base.state.States.DurableRecoveryAwaitingRollback);
            }
            else
            {
                e.Participant.SetCallback(e.Callback, e.CallbackState);
                e.StateMachine.ChangeState(base.state.States.DurableRecoveryReceivedRollback);
            }
        }
    }
}

