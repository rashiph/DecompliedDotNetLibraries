namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class TwoPhaseCommitCoordinator : ITwoPhaseCommitCoordinator
    {
        private EndpointAddress forgottenSource;
        private AsyncCallback politeSendComplete;
        private AsyncCallback sendComplete;
        private ProtocolState state;

        public TwoPhaseCommitCoordinator(ProtocolState state)
        {
            this.state = state;
            this.sendComplete = Fx.ThunkCallback(new AsyncCallback(this.SendComplete));
            this.politeSendComplete = Fx.ThunkCallback(new AsyncCallback(this.PoliteSendComplete));
        }

        public void Aborted(Message message)
        {
            ParticipantEnlistment participant = this.CheckMessage(message, true, false);
            if (participant != null)
            {
                participant.StateMachine.Enqueue(new MsgAbortedEvent(participant));
            }
        }

        private ParticipantEnlistment CheckMessage(Message message, bool fault, bool preparedOrReplay)
        {
            Guid guid;
            ControlProtocol protocol;
            if (!Ports.TryGetEnlistment(message, out guid, out protocol))
            {
                DebugTrace.Trace(TraceLevel.Warning, "Could not read enlistment header from message");
                if (fault)
                {
                    this.SendFault(message, this.state.Faults.InvalidParameters);
                }
                return null;
            }
            Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment = this.state.Lookup.FindEnlistment(guid);
            if (enlistment == null)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Enlistment {0} could not be found", guid);
                if (preparedOrReplay)
                {
                    if (protocol == ControlProtocol.Volatile2PC)
                    {
                        if (DebugTrace.Warning)
                        {
                            DebugTrace.Trace(TraceLevel.Warning, "Received Prepared or Replay from unrecognized volatile participant at {0}", Ports.TryGetFromAddress(message));
                        }
                        if (VolatileParticipantInDoubtRecord.ShouldTrace)
                        {
                            VolatileParticipantInDoubtRecord.Trace(guid, Library.GetReplyToHeader(message.Headers), this.state.ProtocolVersion);
                        }
                        this.SendFault(message, this.state.Faults.UnknownTransaction);
                    }
                    else if (protocol == ControlProtocol.Durable2PC)
                    {
                        this.SendRollback(message);
                    }
                    else
                    {
                        this.SendFault(message, this.state.Faults.InvalidParameters);
                    }
                }
                else if (DebugTrace.Info)
                {
                    DebugTrace.Trace(TraceLevel.Info, "Ignoring message from unrecognized participant at {0}", Ports.TryGetFromAddress(message));
                }
                return null;
            }
            ParticipantEnlistment enlistment2 = enlistment as ParticipantEnlistment;
            if ((enlistment2 == null) || (protocol != enlistment2.ControlProtocol))
            {
                DebugTrace.Trace(TraceLevel.Warning, "Enlistment state does not match message for {0}", guid);
                if (fault)
                {
                    this.SendFault(message, this.state.Faults.InvalidParameters);
                }
                return null;
            }
            if (enlistment2.ParticipantProxy == null)
            {
                DebugTrace.TxTrace(TraceLevel.Warning, enlistment2.EnlistmentId, "Participant enlistment was not correctly recovered");
                if (fault)
                {
                    this.SendFault(message, this.state.Faults.InvalidPolicy);
                }
                return null;
            }
            if (this.state.Service.Security.CheckIdentity(enlistment2.ParticipantProxy, message))
            {
                return enlistment2;
            }
            if (EnlistmentIdentityCheckFailedRecord.ShouldTrace)
            {
                EnlistmentIdentityCheckFailedRecord.Trace(enlistment2.EnlistmentId);
            }
            return null;
        }

        public void Committed(Message message)
        {
            ParticipantEnlistment participant = this.CheckMessage(message, true, false);
            if (participant != null)
            {
                participant.StateMachine.Enqueue(new MsgCommittedEvent(participant));
            }
        }

        private EndpointAddress CreateForgottenSource()
        {
            if (this.forgottenSource == null)
            {
                EnlistmentHeader refParam = new EnlistmentHeader(Guid.Empty, ControlProtocol.None);
                this.forgottenSource = this.state.TwoPhaseCommitCoordinatorListener.CreateEndpointReference(refParam);
            }
            return this.forgottenSource;
        }

        public void Fault(Message message, MessageFault fault)
        {
            ParticipantEnlistment participant = this.CheckMessage(message, false, false);
            if (participant != null)
            {
                this.state.Perf.FaultsReceivedCountPerInterval.Increment();
                participant.StateMachine.Enqueue(new MsgParticipantFaultEvent(participant, fault));
            }
        }

        private void OnPoliteSendComplete(IAsyncResult ar, TwoPhaseCommitParticipantProxy proxy)
        {
            try
            {
                proxy.EndSendMessage(ar);
            }
            catch (WsatSendFailureException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                DebugTrace.TraceSendFailure(exception);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
            }
        }

        private void OnSendComplete(IAsyncResult ar, ParticipantEnlistment participant)
        {
            Exception exception = null;
            try
            {
                participant.ParticipantProxy.EndSendMessage(ar);
            }
            catch (WsatSendFailureException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                DebugTrace.TraceSendFailure(participant.EnlistmentId, exception2);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
                exception = exception2;
            }
            if (exception != null)
            {
                participant.StateMachine.Enqueue(new MsgParticipantSendFailureEvent(participant));
            }
        }

        private void PoliteSendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnPoliteSendComplete(ar, (TwoPhaseCommitParticipantProxy) ar.AsyncState);
            }
        }

        public void Prepared(Message message)
        {
            ParticipantEnlistment participant = this.CheckMessage(message, true, true);
            if (participant != null)
            {
                participant.StateMachine.Enqueue(new MsgPreparedEvent(participant));
            }
        }

        public void ReadOnly(Message message)
        {
            ParticipantEnlistment participant = this.CheckMessage(message, true, false);
            if (participant != null)
            {
                participant.StateMachine.Enqueue(new MsgReadOnlyEvent(participant));
            }
        }

        public void Replay(Message message)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(this.state.ProtocolVersion, base.GetType(), "Replay");
            ParticipantEnlistment participant = this.CheckMessage(message, true, true);
            if (participant != null)
            {
                participant.StateMachine.Enqueue(new MsgReplayEvent(participant));
            }
        }

        public void SendCommit(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Sending Commit to {0} participant at {1}", participant.ControlProtocol, Ports.TryGetAddress(participant.ParticipantProxy));
            }
            IAsyncResult ar = participant.ParticipantProxy.BeginSendCommit(this.sendComplete, participant);
            if (ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, participant);
            }
        }

        public void SendCommit(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                TwoPhaseCommitParticipantProxy proxy = this.state.TryCreateTwoPhaseCommitParticipantProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Commit to unrecognized participant at {0}", Ports.TryGetAddress(proxy));
                        }
                        proxy.From = this.CreateForgottenSource();
                        IAsyncResult ar = proxy.BeginSendCommit(this.politeSendComplete, proxy);
                        if (ar.CompletedSynchronously)
                        {
                            this.OnPoliteSendComplete(ar, proxy);
                        }
                    }
                    finally
                    {
                        proxy.Release();
                    }
                }
            }
        }

        private void SendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, (ParticipantEnlistment) ar.AsyncState);
            }
        }

        private void SendFault(Message message, Microsoft.Transactions.Wsat.Messaging.Fault fault)
        {
            this.SendFault(Library.GetFaultToHeader(message.Headers, this.state.ProtocolVersion), message.Headers.MessageId, fault);
        }

        public void SendFault(EndpointAddress faultTo, UniqueId messageID, Microsoft.Transactions.Wsat.Messaging.Fault fault)
        {
            if (faultTo != null)
            {
                this.state.FaultSender.TrySendTwoPhaseCommitParticipantFault(faultTo, messageID, fault);
            }
        }

        public void SendPrepare(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Sending Prepare to {0} participant at {1}", participant.ControlProtocol, Ports.TryGetAddress(participant.ParticipantProxy));
            }
            IAsyncResult ar = participant.ParticipantProxy.BeginSendPrepare(this.sendComplete, participant);
            if (ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, participant);
            }
        }

        public void SendRollback(ParticipantEnlistment participant)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, participant.EnlistmentId, "Sending Rollback to {0} participant at {1}", participant.ControlProtocol, Ports.TryGetAddress(participant.ParticipantProxy));
            }
            IAsyncResult ar = participant.ParticipantProxy.BeginSendRollback(this.sendComplete, participant);
            if (ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, participant);
            }
        }

        private void SendRollback(Message message)
        {
            this.SendRollback(Library.GetReplyToHeader(message.Headers));
        }

        public void SendRollback(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                TwoPhaseCommitParticipantProxy proxy = this.state.TryCreateTwoPhaseCommitParticipantProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Rollback to unrecognized participant at {0}", Ports.TryGetAddress(proxy));
                        }
                        proxy.From = this.CreateForgottenSource();
                        IAsyncResult ar = proxy.BeginSendRollback(this.politeSendComplete, proxy);
                        if (ar.CompletedSynchronously)
                        {
                            this.OnPoliteSendComplete(ar, proxy);
                        }
                    }
                    finally
                    {
                        proxy.Release();
                    }
                }
            }
        }
    }
}

