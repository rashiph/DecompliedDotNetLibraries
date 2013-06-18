namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.Xml;

    internal class FaultSender
    {
        private AsyncCallback sendFaultComplete;
        private ProtocolState state;

        public FaultSender(ProtocolState state)
        {
            this.state = state;
            this.sendFaultComplete = Fx.ThunkCallback(new AsyncCallback(this.SendFaultComplete));
        }

        private void OnSendFaultComplete(IAsyncResult ar, DatagramProxy proxy)
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

        private void SendFaultComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnSendFaultComplete(ar, (DatagramProxy) ar.AsyncState);
            }
        }

        public void TrySendCompletionParticipantFault(EndpointAddress faultTo, UniqueId messageID, Fault fault)
        {
            DatagramProxy proxy = this.state.TryCreateCompletionParticipantProxy(faultTo);
            if (proxy == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Could not create a proxy to send {0} fault", fault.Code.Name);
                }
            }
            else
            {
                try
                {
                    this.TrySendFault(proxy, messageID, fault);
                }
                finally
                {
                    proxy.Release();
                }
            }
        }

        private void TrySendFault(DatagramProxy proxy, UniqueId messageID, Fault fault)
        {
            if (proxy == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Could not create a proxy to send {0} fault", fault.Code.Name);
                }
            }
            else
            {
                this.state.Perf.FaultsSentCountPerInterval.Increment();
                if (DebugTrace.Info)
                {
                    DebugTrace.Trace(TraceLevel.Info, "Sending {0} fault to {1}", fault.Code.Name, Ports.TryGetAddress(proxy));
                }
                IAsyncResult ar = proxy.BeginSendFault(messageID, fault, this.sendFaultComplete, proxy);
                if (ar.CompletedSynchronously)
                {
                    this.OnSendFaultComplete(ar, proxy);
                }
            }
        }

        public void TrySendTwoPhaseCommitCoordinatorFault(EndpointAddress faultTo, UniqueId messageID, Fault fault)
        {
            DatagramProxy proxy = this.state.TryCreateTwoPhaseCommitCoordinatorProxy(faultTo);
            if (proxy == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Could not create a proxy to send {0} fault", fault.Code.Name);
                }
            }
            else
            {
                try
                {
                    this.TrySendFault(proxy, messageID, fault);
                }
                finally
                {
                    proxy.Release();
                }
            }
        }

        public void TrySendTwoPhaseCommitParticipantFault(EndpointAddress faultTo, UniqueId messageID, Fault fault)
        {
            DatagramProxy proxy = this.state.TryCreateTwoPhaseCommitParticipantProxy(faultTo);
            if (proxy == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Could not create a proxy to send {0} fault", fault.Code.Name);
                }
            }
            else
            {
                try
                {
                    this.TrySendFault(proxy, messageID, fault);
                }
                finally
                {
                    proxy.Release();
                }
            }
        }
    }
}

