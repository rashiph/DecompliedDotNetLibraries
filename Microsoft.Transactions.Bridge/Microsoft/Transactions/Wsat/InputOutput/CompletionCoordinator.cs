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

    internal class CompletionCoordinator : ICompletionCoordinator
    {
        private AsyncCallback politeSendComplete;
        private AsyncCallback sendComplete;
        private ProtocolState state;

        public CompletionCoordinator(ProtocolState state)
        {
            this.state = state;
            this.sendComplete = Fx.ThunkCallback(new AsyncCallback(this.SendComplete));
            this.politeSendComplete = Fx.ThunkCallback(new AsyncCallback(this.PoliteSendComplete));
        }

        private CompletionEnlistment CheckMessage(Message message, bool reply)
        {
            Guid guid;
            if (!Ports.TryGetEnlistment(message, out guid))
            {
                DebugTrace.Trace(TraceLevel.Warning, "Could not read enlistment header from message");
                if (reply)
                {
                    this.SendFault(message, this.state.Faults.InvalidParameters);
                }
                return null;
            }
            Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment = this.state.Lookup.FindEnlistment(guid);
            if (enlistment == null)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Could not find enlistment {0}", guid);
                if (reply)
                {
                    this.SendFault(message, this.state.Faults.InvalidState);
                }
                return null;
            }
            CompletionEnlistment enlistment2 = enlistment as CompletionEnlistment;
            if (enlistment2 == null)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Completion message received for non-completion enlistment {0}", guid);
                if (reply)
                {
                    this.SendFault(message, this.state.Faults.InvalidParameters);
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

        public void Commit(Message message)
        {
            CompletionEnlistment completion = this.CheckMessage(message, true);
            if (completion != null)
            {
                completion.StateMachine.Enqueue(new MsgCompletionCommitEvent(completion));
            }
        }

        public void Fault(Message message, MessageFault fault)
        {
            if (this.CheckMessage(message, false) != null)
            {
                this.state.Perf.FaultsReceivedCountPerInterval.Increment();
            }
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Ignoring {0} fault from completion participant at {1}: {2}", Library.GetFaultCodeName(fault), Ports.TryGetFromAddress(message), Library.GetFaultCodeReason(fault));
            }
        }

        private void OnSendComplete(IAsyncResult ar, CompletionEnlistment completion, CompletionParticipantProxy proxy)
        {
            try
            {
                proxy.EndSendMessage(ar);
            }
            catch (WsatSendFailureException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
                if (completion != null)
                {
                    DebugTrace.TraceSendFailure(completion.EnlistmentId, exception);
                }
                else
                {
                    DebugTrace.TraceSendFailure(exception);
                }
            }
        }

        private void PoliteSendComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                CompletionParticipantProxy asyncState = (CompletionParticipantProxy) ar.AsyncState;
                this.OnSendComplete(ar, null, asyncState);
            }
        }

        public void Rollback(Message message)
        {
            CompletionEnlistment completion = this.CheckMessage(message, true);
            if (completion != null)
            {
                completion.StateMachine.Enqueue(new MsgCompletionRollbackEvent(completion));
            }
        }

        public void SendAborted(CompletionEnlistment completion)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, completion.EnlistmentId, "Sending Aborted to completion participant at {0}", Ports.TryGetAddress(completion.ParticipantProxy));
            }
            IAsyncResult ar = completion.ParticipantProxy.BeginSendAborted(this.sendComplete, completion);
            if (ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, completion, completion.ParticipantProxy);
            }
        }

        public void SendAborted(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                CompletionParticipantProxy proxy = this.state.TryCreateCompletionParticipantProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Aborted to unrecognized completion participant at {0}", Ports.TryGetAddress(proxy));
                        }
                        IAsyncResult ar = proxy.BeginSendAborted(this.politeSendComplete, proxy);
                        if (ar.CompletedSynchronously)
                        {
                            this.OnSendComplete(ar, null, proxy);
                        }
                    }
                    finally
                    {
                        proxy.Release();
                    }
                }
            }
        }

        public void SendCommitted(CompletionEnlistment completion)
        {
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, completion.EnlistmentId, "Sending Committed to completion participant at {0}", Ports.TryGetAddress(completion.ParticipantProxy));
            }
            IAsyncResult ar = completion.ParticipantProxy.BeginSendCommitted(this.sendComplete, completion);
            if (ar.CompletedSynchronously)
            {
                this.OnSendComplete(ar, completion, completion.ParticipantProxy);
            }
        }

        public void SendCommitted(EndpointAddress sendTo)
        {
            if (sendTo != null)
            {
                CompletionParticipantProxy proxy = this.state.TryCreateCompletionParticipantProxy(sendTo);
                if (proxy != null)
                {
                    try
                    {
                        if (DebugTrace.Info)
                        {
                            DebugTrace.Trace(TraceLevel.Info, "Sending Committed to unrecognized completion participant at {0}", Ports.TryGetAddress(proxy));
                        }
                        IAsyncResult ar = proxy.BeginSendCommitted(this.politeSendComplete, proxy);
                        if (ar.CompletedSynchronously)
                        {
                            this.OnSendComplete(ar, null, proxy);
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
                CompletionEnlistment asyncState = (CompletionEnlistment) ar.AsyncState;
                this.OnSendComplete(ar, asyncState, asyncState.ParticipantProxy);
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
                this.state.FaultSender.TrySendCompletionParticipantFault(faultTo, messageID, fault);
            }
        }
    }
}

