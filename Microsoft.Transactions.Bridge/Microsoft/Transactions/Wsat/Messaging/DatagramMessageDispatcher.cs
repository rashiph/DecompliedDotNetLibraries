namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class DatagramMessageDispatcher
    {
        private ProtocolVersion protocolVersion;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DatagramMessageDispatcher(ProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
        }

        protected abstract DatagramProxy CreateFaultProxy(EndpointAddress to);
        protected void OnMessageException(Message message, CommunicationException exception)
        {
            DebugTrace.Trace(TraceLevel.Warning, "{0} - {1} reading datagram with action {2}: {3}", base.GetType().Name, exception.GetType().Name, message.Headers.Action, exception.Message);
            EndpointAddress faultToHeader = Library.GetFaultToHeader(message.Headers, this.protocolVersion);
            if (faultToHeader == null)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Ignoring invalid datagram - a fault-to header could not be obtained");
            }
            else
            {
                DatagramProxy proxy = null;
                try
                {
                    proxy = this.CreateFaultProxy(faultToHeader);
                }
                catch (CreateChannelFailureException exception2)
                {
                    Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                    DebugTrace.Trace(TraceLevel.Warning, "Ignoring invalid datagram: {0}", exception2.Message);
                }
                if (proxy != null)
                {
                    try
                    {
                        IAsyncResult ar = proxy.BeginSendFault(message.Headers.MessageId, Faults.Version(this.protocolVersion).InvalidParameters, null, null);
                        proxy.EndSendMessage(ar);
                        if (DebugTrace.Warning)
                        {
                            DebugTrace.Trace(TraceLevel.Warning, "Sent InvalidParameters fault to {0}", proxy.To.Uri);
                        }
                    }
                    catch (WsatSendFailureException exception3)
                    {
                        Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                        if (DebugTrace.Warning)
                        {
                            DebugTrace.Trace(TraceLevel.Warning, "{0} sending InvalidParameters fault to {1}: {2}", exception3.GetType().Name, proxy.To.Uri, exception3.Message);
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

