namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class CompletionParticipantDispatcher : DatagramMessageDispatcher
    {
        private ICompletionParticipant dispatch;
        private CoordinationService service;

        public CompletionParticipantDispatcher(CoordinationService service, ICompletionParticipant dispatch) : base(service.ProtocolVersion)
        {
            this.service = service;
            this.dispatch = dispatch;
        }

        public void Aborted(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching completion Aborted message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                AbortedMessage.ReadFrom(message, this.service.ProtocolVersion);
                this.dispatch.Aborted(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion Aborted message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public void Committed(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching completion Committed message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                CommittedMessage.ReadFrom(message, this.service.ProtocolVersion);
                this.dispatch.Committed(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion Committed message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        protected override DatagramProxy CreateFaultProxy(EndpointAddress to)
        {
            return this.service.CreateCompletionCoordinatorProxy(to, null);
        }

        private void Fault(Message message)
        {
            try
            {
                MessageFault fault = MessageFault.CreateFault(message, 0x10000);
                if (DebugTrace.Verbose)
                {
                    FaultCode baseFaultCode = Library.GetBaseFaultCode(fault);
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching coordinator completion fault {0}", (baseFaultCode == null) ? "unknown" : baseFaultCode.Name);
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                this.dispatch.Fault(message, fault);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion fault from coordinator: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public static IWSCompletionParticipant Instance(CoordinationService service, ICompletionParticipant dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion(service.ProtocolVersion, typeof(CompletionParticipantDispatcher), "Instance");
            switch (service.ProtocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new CompletionParticipantDispatcher10(service, dispatch);

                case ProtocolVersion.Version11:
                    return new CompletionParticipantDispatcher11(service, dispatch);
            }
            return null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WsaFault(Message message)
        {
            this.Fault(message);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WsatFault(Message message)
        {
            this.Fault(message);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WscoorFault(Message message)
        {
            this.Fault(message);
        }
    }
}

