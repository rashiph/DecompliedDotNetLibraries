namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class TwoPhaseCommitCoordinatorDispatcher : DatagramMessageDispatcher
    {
        private ITwoPhaseCommitCoordinator dispatch;
        private ProtocolVersion protocolVersion;
        private CoordinationService service;

        public TwoPhaseCommitCoordinatorDispatcher(CoordinationService service, ITwoPhaseCommitCoordinator dispatch) : base(service.ProtocolVersion)
        {
            this.service = service;
            this.dispatch = dispatch;
            this.protocolVersion = service.ProtocolVersion;
        }

        public void Aborted(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching 2PC Aborted message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                AbortedMessage.ReadFrom(message, this.protocolVersion);
                this.dispatch.Aborted(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching 2PC Aborted message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public void Committed(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching 2PC Committed message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                CommittedMessage.ReadFrom(message, this.protocolVersion);
                this.dispatch.Committed(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching 2PC Committed message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        protected override DatagramProxy CreateFaultProxy(EndpointAddress to)
        {
            return this.service.CreateTwoPhaseCommitParticipantProxy(to, null);
        }

        private void Fault(Message message)
        {
            try
            {
                MessageFault fault = MessageFault.CreateFault(message, 0x10000);
                if (DebugTrace.Verbose)
                {
                    FaultCode baseFaultCode = Library.GetBaseFaultCode(fault);
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching participant 2PC fault {0}", (baseFaultCode == null) ? "unknown" : baseFaultCode.Name);
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
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching 2PC fault from participant: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public static IWSTwoPhaseCommitCoordinator Instance(CoordinationService service, ITwoPhaseCommitCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion(service.ProtocolVersion, typeof(TwoPhaseCommitCoordinatorDispatcher), "V");
            switch (service.ProtocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new TwoPhaseCommitCoordinatorDispatcher10(service, dispatch);

                case ProtocolVersion.Version11:
                    return new TwoPhaseCommitCoordinatorDispatcher11(service, dispatch);
            }
            return null;
        }

        public void Prepared(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching 2PC Prepared message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                PreparedMessage.ReadFrom(message, this.protocolVersion);
                this.dispatch.Prepared(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching 2PC Prepared message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public void ReadOnly(Message message)
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching 2PC ReadOnly message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                ReadOnlyMessage.ReadFrom(message, this.protocolVersion);
                this.dispatch.ReadOnly(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching 2PC ReadOnly message: {1}", exception2.GetType().Name, exception2);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception2);
            }
        }

        public void Replay(Message message)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(this.protocolVersion, typeof(TwoPhaseCommitCoordinatorDispatcher), "constr");
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching 2PC Replay message");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                    }
                }
                ReplayMessage.ReadFrom(message, this.protocolVersion);
                this.dispatch.Replay(message);
            }
            catch (CommunicationException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(message, exception);
            }
            catch (Exception exception2)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching Replay message: {1}", exception2.GetType().Name, exception2);
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

