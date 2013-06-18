namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    internal class TwoPhaseCommitParticipant : ITwoPhaseCommitParticipant
    {
        private AsyncCallback durableSendComplete;
        private AsyncCallback politeSendComplete;
        private ProtocolState state;
        private AsyncCallback volatileSendComplete;

        public TwoPhaseCommitParticipant(ProtocolState state)
        {
            this.state = state;
            this.durableSendComplete = Fx.ThunkCallback(new AsyncCallback(this.DurableSendComplete));
            this.volatileSendComplete = Fx.ThunkCallback(new AsyncCallback(this.VolatileSendComplete));
            this.politeSendComplete = Fx.ThunkCallback(new AsyncCallback(this.PoliteSendComplete));
        }

        private bool CheckMessage(Message message, bool fault, out CoordinatorEnlistment durableCoordinator, out VolatileCoordinatorEnlistment volatileCoordinator)
        {
            Guid guid;
            TwoPhaseCommitCoordinatorProxy coordinatorProxy;
            durableCoordinator = null;
            volatileCoordinator = null;
            if (!Ports.TryGetEnlistment(message, out guid))
            {
                DebugTrace.Trace(TraceLevel.Warning, "Could not read enlistment header from message");
                if (fault)
                {
                    this.SendFault(message, this.state.Faults.InvalidParameters);
                }
                return false;
            }
            Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment = this.state.Lookup.FindEnlistment(guid);
            if (enlistment == null)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Could not find enlistment {0}", guid);
                return true;
            }
            durableCoordinator = enlistment as CoordinatorEnlistment;
            if (durableCoordinator == null)
            {
                volatileCoordinator = enlistment as VolatileCoordinatorEnlistment;
                if (volatileCoordinator == null)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "2PC message received for non-2PC enlistment {0}", guid);
                    if (fault)
                    {
                        this.SendFault(message, this.state.Faults.InvalidParameters);
                    }
                    return false;
                }
                coordinatorProxy = volatileCoordinator.CoordinatorProxy;
            }
            else
            {
                coordinatorProxy = durableCoordinator.CoordinatorProxy;
            }
            if (coordinatorProxy == null)
            {
                if ((durableCoordinator != null) && object.ReferenceEquals(durableCoordinator.StateMachine.State, this.state.States.CoordinatorFailedRecovery))
                {
                    DebugTrace.TxTrace(TraceLevel.Warning, enlistment.EnlistmentId, "Coordinator enlistment was not correctly recovered");
                    if (fault)
                    {
                        this.SendFault(message, this.state.Faults.InvalidPolicy);
                    }
                    return false;
                }
                if (DebugTrace.Warning)
                {
                    DebugTrace.TxTrace(TraceLevel.Warning, enlistment.EnlistmentId, "Received premature message with action {0}", message.Headers.Action);
                }
                if (fault)
                {
                    this.SendFault(message, this.state.Faults.InvalidState);
                }
                return false;
            }
            if (this.state.Service.Security.CheckIdentity(coordinatorProxy, message))
            {
                return true;
            }
            if (EnlistmentIdentityCheckFailedRecord.ShouldTrace)
            {
                EnlistmentIdentityCheckFailedRecord.Trace(enlistment.EnlistmentId);
            }
            return false;
        }

        public void Commit(Message message)
        {
            CoordinatorEnlistment enlistment;
            VolatileCoordinatorEnlistment enlistment2;
            if (this.CheckMessage(message, true, out enlistment, out enlistment2))
            {
                if (enlistment != null)
                {
                    enlistment.StateMachine.Enqueue(new MsgDurableCommitEvent(enlistment));
                }
                else if (enlistment2 != null)
                {
                    enlistment2.StateMachine.Enqueue(new MsgVolatileCommitEvent(enlistment2));
                }
                else
                {
                    this.SendCommitted(message);
                }
            }
        }

        private void DurableSendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, (CoordinatorEnlistment) ar.AsyncState);
            }
        }

        public void Fault(Message message, MessageFault fault)
        {
            CoordinatorEnlistment enlistment;
            VolatileCoordinatorEnlistment enlistment2;
            if (this.CheckMessage(message, false, out enlistment, out enlistment2))
            {
                if (enlistment != null)
                {
                    enlistment.StateMachine.Enqueue(new MsgDurableCoordinatorFaultEvent(enlistment, fault));
                }
                else if (enlistment2 != null)
                {
                    enlistment2.StateMachine.Enqueue(new MsgVolatileCoordinatorFaultEvent(enlistment2, fault));
                }
                else if (DebugTrace.Info)
                {
                    DebugTrace.Trace(TraceLevel.Info, "Ignoring {0} fault from unrecognized coordinator at {1}: {2}", Library.GetFaultCodeName(fault), Ports.TryGetFromAddress(message), Library.GetFaultCodeReason(fault));
                }
                this.state.Perf.FaultsReceivedCountPerInterval.Increment();
            }
        }

        private void OnDurableSendComplete(IAsyncResult ar, CoordinatorEnlistment coordinator)
        {
            Exception exception = null;
            try
            {
                coordinator.CoordinatorProxy.EndSendMessage(ar);
            }
            catch (WsatSendFailureException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                DebugTrace.TraceSendFailure(coordinator.EnlistmentId, exception2);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
                exception = exception2;
            }
            if (exception != null)
            {
                coordinator.StateMachine.Enqueue(new MsgDurableCoordinatorSendFailureEvent(coordinator));
            }
        }

        private void OnPoliteSendComplete(IAsyncResult ar, TwoPhaseCommitCoordinatorProxy proxy)
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

        private void OnVolatileSendComplete(IAsyncResult ar, VolatileCoordinatorEnlistment volatileCoordinator)
        {
            Exception exception = null;
            try
            {
                volatileCoordinator.CoordinatorProxy.EndSendMessage(ar);
            }
            catch (WsatSendFailureException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                DebugTrace.TraceSendFailure(volatileCoordinator.EnlistmentId, exception2);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
                exception = exception2;
            }
            if (exception != null)
            {
                volatileCoordinator.StateMachine.Enqueue(new MsgVolatileCoordinatorSendFailureEvent(volatileCoordinator));
            }
        }

        private void PoliteSendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnPoliteSendComplete(ar, (TwoPhaseCommitCoordinatorProxy) ar.AsyncState);
            }
        }

        public void Prepare(Message message)
        {
            CoordinatorEnlistment enlistment;
            VolatileCoordinatorEnlistment enlistment2;
            if (this.CheckMessage(message, true, out enlistment, out enlistment2))
            {
                if (enlistment != null)
                {
                    enlistment.StateMachine.Enqueue(new MsgDurablePrepareEvent(enlistment));
                }
                else if (enlistment2 != null)
                {
                    enlistment2.StateMachine.Enqueue(new MsgVolatilePrepareEvent(enlistment2));
                }
                else
                {
                    this.SendAborted(message);
                }
            }
        }

        public void Rollback(Message message)
        {
            CoordinatorEnlistment enlistment;
            VolatileCoordinatorEnlistment enlistment2;
            if (this.CheckMessage(message, true, out enlistment, out enlistment2))
            {
                if (enlistment != null)
                {
                    enlistment.StateMachine.Enqueue(new MsgDurableRollbackEvent(enlistment));
                }
                else if (enlistment2 != null)
                {
                    enlistment2.StateMachine.Enqueue(new MsgVolatileRollbackEvent(enlistment2));
                }
                else
                {
                    this.SendAborted(message);
                }
            }
        }

        private void SendAborted(Message message)
        {
            this.SendAborted(Library.GetReplyToHeader(message.Headers));
        }

        public void SendAborted(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                TwoPhaseCommitCoordinatorProxy proxy = this.state.TryCreateTwoPhaseCommitCoordinatorProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Aborted to unrecognized coordinator at {0}", Ports.TryGetAddress(proxy));
                        }
                        IAsyncResult ar = proxy.BeginSendAborted(this.politeSendComplete, proxy);
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

        public void SendCommitted(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Committed to durable coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendCommitted(this.durableSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, coordinator);
            }
        }

        private void SendCommitted(Message message)
        {
            this.SendCommitted(Library.GetReplyToHeader(message.Headers));
        }

        public void SendCommitted(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                TwoPhaseCommitCoordinatorProxy proxy = this.state.TryCreateTwoPhaseCommitCoordinatorProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Committed to unrecognized coordinator at {0}", Ports.TryGetAddress(proxy));
                        }
                        IAsyncResult ar = proxy.BeginSendCommitted(this.politeSendComplete, proxy);
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

        public void SendDurableAborted(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Aborted to durable coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendAborted(this.durableSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, coordinator);
            }
        }

        public void SendDurableReadOnly(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending ReadOnly to durable coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendReadOnly(this.durableSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, coordinator);
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
                this.state.FaultSender.TrySendTwoPhaseCommitCoordinatorFault(faultTo, messageID, fault);
            }
        }

        public void SendPrepared(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Prepared to durable coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendPrepared(this.durableSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, coordinator);
            }
        }

        public void SendReadOnly(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                TwoPhaseCommitCoordinatorProxy proxy = this.state.TryCreateTwoPhaseCommitCoordinatorProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending ReadOnly to unrecognized participant at {0}", Ports.TryGetAddress(proxy));
                        }
                        IAsyncResult ar = proxy.BeginSendReadOnly(this.politeSendComplete, proxy);
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

        public void SendRecoverMessage(CoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Replay to durable coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendRecoverMessage(this.durableSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnDurableSendComplete(ar, coordinator);
            }
        }

        public void SendVolatileAborted(VolatileCoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Aborted to volatile coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendAborted(this.volatileSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnVolatileSendComplete(ar, coordinator);
            }
        }

        public void SendVolatileReadOnly(VolatileCoordinatorEnlistment coordinator)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending ReadOnly to volatile coordinator at {0}", Ports.TryGetAddress(coordinator.CoordinatorProxy));
            }
            IAsyncResult ar = coordinator.CoordinatorProxy.BeginSendReadOnly(this.volatileSendComplete, coordinator);
            if (ar.CompletedSynchronously)
            {
                this.OnVolatileSendComplete(ar, coordinator);
            }
        }

        private void VolatileSendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnVolatileSendComplete(ar, (VolatileCoordinatorEnlistment) ar.AsyncState);
            }
        }
    }
}

