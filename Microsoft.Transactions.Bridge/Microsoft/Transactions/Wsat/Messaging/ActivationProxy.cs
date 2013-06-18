namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class ActivationProxy : RequestReplyProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivationProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to)
        {
        }

        public IAsyncResult BeginSendCreateCoordinationContext(ref CreateCoordinationContext create, AsyncCallback callback, object state)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Sending CreateCoordinationContext to {0}", base.to.Uri);
            }
            Message message = this.CreateCreateCoordinationContextMessage(ref create);
            return base.BeginSendRequest(message, callback, state);
        }

        private CreateCoordinationContextMessage CreateCreateCoordinationContextMessage(ref CreateCoordinationContext create)
        {
            CreateCoordinationContextMessage message = new CreateCoordinationContextMessage(base.messageVersion, ref create);
            if (create.IssuedToken != null)
            {
                CoordinationServiceSecurity.AddIssuedToken(message, create.IssuedToken);
            }
            return message;
        }

        public CreateCoordinationContextResponse EndSendCreateCoordinationContext(IAsyncResult ar)
        {
            CreateCoordinationContextResponse response;
            try
            {
                Message message = base.EndSendRequest(ar, base.coordinationStrings.CreateCoordinationContextResponseAction);
                using (message)
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Dispatching CreateCoordinationContextResponse reply");
                        if (DebugTrace.Pii)
                        {
                            DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                        }
                    }
                    response = new CreateCoordinationContextResponse(message, base.protocolVersion);
                }
            }
            catch (CommunicationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatReceiveFailureException(exception));
            }
            return response;
        }

        public CreateCoordinationContextResponse SendCreateCoordinationContext(ref CreateCoordinationContext create)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Sending CreateCoordinationContext to {0}", base.to.Uri);
            }
            Message message = this.CreateCreateCoordinationContextMessage(ref create);
            Message message2 = base.SendRequest(message, base.coordinationStrings.CreateCoordinationContextResponseAction);
            using (message2)
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching CreateCoordinationContextResponse reply");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message2));
                    }
                }
                return new CreateCoordinationContextResponse(message2, base.protocolVersion);
            }
        }

        public static void SendCreateCoordinationContextResponse(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, ref CreateCoordinationContextResponse response)
        {
            Message message = new CreateCoordinationContextResponseMessage(result.MessageVersion, ref response);
            if (response.IssuedToken != null)
            {
                CoordinationServiceSecurity.AddIssuedToken(message, response.IssuedToken);
            }
            result.Finished(message);
        }

        public static void SendFaultResponse(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            Library.SendFaultResponse(result, fault);
        }
    }
}

