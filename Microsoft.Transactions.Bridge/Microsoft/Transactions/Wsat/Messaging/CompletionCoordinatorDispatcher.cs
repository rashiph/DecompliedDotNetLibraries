namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class CompletionCoordinatorDispatcher : DatagramMessageDispatcher
    {
        private ICompletionCoordinator dispatch;
        private CoordinationService service;

        public CompletionCoordinatorDispatcher(CoordinationService service, ICompletionCoordinator dispatch) : base(service.ProtocolVersion)
        {
            this.service = service;
            this.dispatch = dispatch;
        }

        public void Commit(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching completion Commit message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                CommitMessage.ReadFrom(message, this.service.ProtocolVersion);
                this.dispatch.Commit(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion Commit message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        protected override DatagramProxy CreateFaultProxy(EndpointAddress to)
        {
            return this.service.CreateCompletionParticipantProxy(to);
        }

        private void Fault(Message message)
        {
            try
            {
                MessageFault fault = MessageFault.CreateFault(message, 0x10000);
                if (DebugTrace.Verbose)
                {
                    FaultCode baseFaultCode = Library.GetBaseFaultCode(fault);
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching participant completion fault {0}", (baseFaultCode == null) ? "unknown" : baseFaultCode.Name);
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
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion fault from participant: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public static IWSCompletionCoordinator Instance(CoordinationService service, ICompletionCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion(service.ProtocolVersion, typeof(CompletionCoordinatorDispatcher), "Instance");
            switch (service.ProtocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new CompletionCoordinatorDispatcher10(service, dispatch);

                case ProtocolVersion.Version11:
                    return new CompletionCoordinatorDispatcher11(service, dispatch);
            }
            return null;
        }

        public void Rollback(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching completion Rollback message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                RollbackMessage.ReadFrom(message, this.service.ProtocolVersion);
                this.dispatch.Rollback(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching completion Rollback message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
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

