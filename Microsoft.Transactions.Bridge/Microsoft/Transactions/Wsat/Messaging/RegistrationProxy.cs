namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class RegistrationProxy : RequestReplyProxy
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RegistrationProxy(CoordinationService coordination, EndpointAddress to) : base(coordination, to)
        {
        }

        public IAsyncResult BeginSendRegister(ref Register register, AsyncCallback callback, object state)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Sending Register to {0}", base.to.Uri);
            }
            Message message = this.CreateRegisterMessage(ref register);
            return base.BeginSendRequest(message, callback, state);
        }

        private RegisterMessage CreateRegisterMessage(ref Register register)
        {
            RegisterMessage message = new RegisterMessage(base.messageVersion, ref register);
            if (register.SupportingToken != null)
            {
                CoordinationServiceSecurity.AddSupportingToken(message, register.SupportingToken);
            }
            return message;
        }

        public RegisterResponse EndSendRegister(IAsyncResult ar)
        {
            RegisterResponse response;
            try
            {
                Message message = base.EndSendRequest(ar, base.coordinationStrings.RegisterResponseAction);
                using (message)
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Dispatching RegisterResponse reply");
                        if (DebugTrace.Pii)
                        {
                            DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message));
                        }
                    }
                    response = new RegisterResponse(message, base.protocolVersion);
                }
            }
            catch (CommunicationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WsatReceiveFailureException(exception));
            }
            return response;
        }

        protected override IChannelFactory<IRequestReplyService> SelectChannelFactory(out MessageVersion messageVersion)
        {
            messageVersion = base.coordinationService.InteropRegistrationBinding.MessageVersion;
            return base.coordinationService.InteropRegistrationChannelFactory;
        }

        public static void SendFaultResponse(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            Library.SendFaultResponse(result, fault);
        }

        public RegisterResponse SendRegister(ref Register register)
        {
            if (DebugTrace.Verbose)
            {
                DebugTrace.Trace(TraceLevel.Verbose, "Sending Register to {0}", base.to.Uri);
            }
            Message message = this.CreateRegisterMessage(ref register);
            Message message2 = base.SendRequest(message, base.coordinationStrings.RegisterResponseAction);
            using (message2)
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Dispatching RegisterResponse reply");
                    if (DebugTrace.Pii)
                    {
                        DebugTrace.TracePii(TraceLevel.Verbose, "Sender is {0}", CoordinationServiceSecurity.GetSenderName(message2));
                    }
                }
                return new RegisterResponse(message2, base.protocolVersion);
            }
        }

        public static void SendRegisterResponse(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, ref RegisterResponse response)
        {
            Message reply = new RegisterResponseMessage(result.MessageVersion, ref response);
            result.Finished(reply);
        }
    }
}

