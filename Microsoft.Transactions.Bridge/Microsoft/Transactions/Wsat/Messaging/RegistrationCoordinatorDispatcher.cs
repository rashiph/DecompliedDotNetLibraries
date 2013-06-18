namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class RegistrationCoordinatorDispatcher : RequestMessageDispatcher
    {
        private IRegistrationCoordinator dispatch;
        private CoordinationService service;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RegistrationCoordinatorDispatcher(CoordinationService service, IRegistrationCoordinator dispatch)
        {
            this.service = service;
            this.dispatch = dispatch;
        }

        public IAsyncResult BeginRegister(Message message, AsyncCallback callback, object state)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Dispatching Register request");
                if (DebugTrace.Pii)
                {
                    DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                }
            }
            Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result = new Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult(message, callback, state);
            try
            {
                this.dispatch.Register(message, result);
            }
            catch (InvalidMessageException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                base.OnMessageException(result, message, exception, Faults.Version(this.service.ProtocolVersion).InvalidParameters);
            }
            catch (CommunicationException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                base.OnMessageException(result, message, exception2, Faults.Version(this.service.ProtocolVersion).RegistrationDispatchFailed);
            }
            catch (Exception exception3)
            {
                DebugTrace.Trace(TraceLevel.Error, "Unhandled exception {0} dispatching Register message: {1}", exception3.GetType().Name, exception3);
                Microsoft.Transactions.Bridge.DiagnosticUtility.InvokeFinalHandler(exception3);
            }
            return result;
        }

        public Message EndRegister(IAsyncResult ar)
        {
            Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result = (Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult) ar;
            result.End();
            return result.Reply;
        }

        public static IWSRegistrationCoordinator Instance(CoordinationService service, IRegistrationCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion(service.ProtocolVersion, typeof(RegistrationCoordinatorDispatcher), "V");
            switch (service.ProtocolVersion)
            {
                case ProtocolVersion.Version10:
                    return new RegistrationCoordinatorDispatcher10(service, dispatch);

                case ProtocolVersion.Version11:
                    return new RegistrationCoordinatorDispatcher11(service, dispatch);
            }
            return null;
        }

        protected override void SendFaultReply(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            RegistrationProxy.SendFaultResponse(result, fault);
        }
    }
}

