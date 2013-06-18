namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal abstract class SupportingTokenChannel<TChannel> : ChannelBase where TChannel: IChannel
    {
        protected TChannel innerChannel;
        private ProtocolVersion protocolVersion;
        private static System.ServiceModel.Security.SecurityStandardsManager securityStandardsManager;
        private SupportingTokenSecurityTokenResolver tokenResolver;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SupportingTokenChannel(ChannelManagerBase manager, TChannel innerChannel, SupportingTokenSecurityTokenResolver tokenResolver, ProtocolVersion protocolVersion) : base(manager)
        {
            this.innerChannel = innerChannel;
            this.tokenResolver = tokenResolver;
            this.protocolVersion = protocolVersion;
        }

        public override T GetProperty<T>() where T: class
        {
            return this.innerChannel.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            this.innerChannel.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginClose(timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerChannel.BeginOpen(timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerChannel.Close(timeout);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerChannel.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerChannel.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerChannel.Open(timeout);
        }

        protected void OnReceive(Message message)
        {
            DebugTrace.TraceEnter(this, "OnReceive");
            if ((message != null) && !this.tokenResolver.FaultInSupportingToken(message))
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Failed to fault in SCT for supporting token signature");
                }
                Fault invalidParameters = Faults.Version(this.protocolVersion).InvalidParameters;
                if (message.Headers.MessageId != null)
                {
                    if (DebugTrace.Verbose)
                    {
                        DebugTrace.Trace(TraceLevel.Verbose, "Attempting to send {0} fault", invalidParameters.Code.Name);
                    }
                    Message reply = Library.CreateFaultMessage(message.Headers.MessageId, message.Version, invalidParameters);
                    RequestReplyCorrelator.AddressReply(reply, new RequestReplyCorrelator.ReplyToInfo(message));
                    SendSecurityHeader header = SupportingTokenChannel<TChannel>.SecurityStandardsManager.CreateSendSecurityHeader(reply, string.Empty, true, false, SecurityAlgorithmSuite.Default, MessageDirection.Output);
                    header.RequireMessageProtection = false;
                    header.AddTimestamp(SecurityProtocolFactory.defaultTimestampValidityDuration);
                    reply = header.SetupExecution();
                    this.TrySendFaultReply(reply);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(Microsoft.Transactions.SR.GetString("SupportingTokenSignatureExpected")));
            }
            DebugTrace.TraceLeave(this, "OnReceive");
        }

        protected abstract void TrySendFaultReply(Message faultMessage);

        private static System.ServiceModel.Security.SecurityStandardsManager SecurityStandardsManager
        {
            get
            {
                if (SupportingTokenChannel<TChannel>.securityStandardsManager == null)
                {
                    SupportingTokenChannel<TChannel>.securityStandardsManager = new System.ServiceModel.Security.SecurityStandardsManager(MessageSecurityVersion.Default, WSSecurityTokenSerializer.DefaultInstance);
                }
                return SupportingTokenChannel<TChannel>.securityStandardsManager;
            }
        }
    }
}

