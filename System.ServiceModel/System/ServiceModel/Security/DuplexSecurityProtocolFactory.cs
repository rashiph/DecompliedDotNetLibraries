namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class DuplexSecurityProtocolFactory : SecurityProtocolFactory
    {
        private SecurityProtocolFactory forwardProtocolFactory;
        private bool requireSecurityOnBothDuplexDirections;
        private SecurityProtocolFactory reverseProtocolFactory;

        public DuplexSecurityProtocolFactory()
        {
            this.requireSecurityOnBothDuplexDirections = true;
        }

        public DuplexSecurityProtocolFactory(SecurityProtocolFactory forwardProtocolFactory, SecurityProtocolFactory reverseProtocolFactory) : this()
        {
            this.forwardProtocolFactory = forwardProtocolFactory;
            this.reverseProtocolFactory = reverseProtocolFactory;
        }

        public override EndpointIdentity GetIdentityOfSelf()
        {
            SecurityProtocolFactory protocolFactoryForIncomingMessages = this.ProtocolFactoryForIncomingMessages;
            if (protocolFactoryForIncomingMessages != null)
            {
                return protocolFactoryForIncomingMessages.GetIdentityOfSelf();
            }
            return base.GetIdentityOfSelf();
        }

        public override void OnAbort()
        {
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.Close(true, TimeSpan.Zero);
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.Close(true, TimeSpan.Zero);
            }
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.Close(false, helper.RemainingTime());
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.Close(false, helper.RemainingTime());
            }
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            SecurityProtocolFactory protocolFactoryForOutgoingMessages = this.ProtocolFactoryForOutgoingMessages;
            SecurityProtocolFactory protocolFactoryForIncomingMessages = this.ProtocolFactoryForIncomingMessages;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            SecurityProtocol outgoingProtocol = (protocolFactoryForOutgoingMessages == null) ? null : protocolFactoryForOutgoingMessages.CreateSecurityProtocol(target, via, listenerSecurityState, false, helper.RemainingTime());
            return new DuplexSecurityProtocol(outgoingProtocol, (protocolFactoryForIncomingMessages == null) ? null : protocolFactoryForIncomingMessages.CreateSecurityProtocol(null, null, listenerSecurityState, false, helper.RemainingTime()));
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if ((this.ForwardProtocolFactory != null) && object.ReferenceEquals(this.ForwardProtocolFactory, this.ReverseProtocolFactory))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("ReverseProtocolFactory", System.ServiceModel.SR.GetString("SameProtocolFactoryCannotBeSetForBothDuplexDirections"));
            }
            if (this.forwardProtocolFactory != null)
            {
                this.forwardProtocolFactory.ListenUri = base.ListenUri;
            }
            if (this.reverseProtocolFactory != null)
            {
                this.reverseProtocolFactory.ListenUri = base.ListenUri;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.Open(this.ForwardProtocolFactory, base.ActAsInitiator, "ForwardProtocolFactory", helper.RemainingTime());
            this.Open(this.ReverseProtocolFactory, !base.ActAsInitiator, "ReverseProtocolFactory", helper.RemainingTime());
        }

        private void Open(SecurityProtocolFactory factory, bool actAsInitiator, string propertyName, TimeSpan timeout)
        {
            if (factory != null)
            {
                factory.Open(actAsInitiator, timeout);
            }
            else if (this.RequireSecurityOnBothDuplexDirections)
            {
                base.OnPropertySettingsError(propertyName, true);
            }
        }

        public SecurityProtocolFactory ForwardProtocolFactory
        {
            get
            {
                return this.forwardProtocolFactory;
            }
            set
            {
                base.ThrowIfImmutable();
                this.forwardProtocolFactory = value;
            }
        }

        private SecurityProtocolFactory ProtocolFactoryForIncomingMessages
        {
            get
            {
                if (!base.ActAsInitiator)
                {
                    return this.ForwardProtocolFactory;
                }
                return this.ReverseProtocolFactory;
            }
        }

        private SecurityProtocolFactory ProtocolFactoryForOutgoingMessages
        {
            get
            {
                if (!base.ActAsInitiator)
                {
                    return this.ReverseProtocolFactory;
                }
                return this.ForwardProtocolFactory;
            }
        }

        public bool RequireSecurityOnBothDuplexDirections
        {
            get
            {
                return this.requireSecurityOnBothDuplexDirections;
            }
            set
            {
                base.ThrowIfImmutable();
                this.requireSecurityOnBothDuplexDirections = value;
            }
        }

        public SecurityProtocolFactory ReverseProtocolFactory
        {
            get
            {
                return this.reverseProtocolFactory;
            }
            set
            {
                base.ThrowIfImmutable();
                this.reverseProtocolFactory = value;
            }
        }

        public override bool SupportsDuplex
        {
            get
            {
                return true;
            }
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return ((((this.ForwardProtocolFactory != null) && this.ForwardProtocolFactory.SupportsReplayDetection) && (this.ReverseProtocolFactory != null)) && this.ReverseProtocolFactory.SupportsReplayDetection);
            }
        }

        public override bool SupportsRequestReply
        {
            get
            {
                return false;
            }
        }

        private sealed class DuplexSecurityProtocol : SecurityProtocol
        {
            private readonly SecurityProtocol incomingProtocol;
            private readonly SecurityProtocol outgoingProtocol;

            public DuplexSecurityProtocol(SecurityProtocol outgoingProtocol, SecurityProtocol incomingProtocol) : base(incomingProtocol.SecurityProtocolFactory, null, null)
            {
                this.outgoingProtocol = outgoingProtocol;
                this.incomingProtocol = incomingProtocol;
            }

            public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.BeginSecureOutgoingMessage(message, timeout, callback, state);
                }
                return new CompletedAsyncResult<Message>(message, callback, state);
            }

            public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.BeginSecureOutgoingMessage(message, timeout, correlationState, callback, state);
                }
                return new CompletedAsyncResult<Message, SecurityProtocolCorrelationState>(message, null, callback, state);
            }

            public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.EndSecureOutgoingMessage(result, out message);
                }
                else
                {
                    message = CompletedAsyncResult<Message>.End(result);
                }
            }

            public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.EndSecureOutgoingMessage(result, out message, out newCorrelationState);
                }
                else
                {
                    message = CompletedAsyncResult<Message, SecurityProtocolCorrelationState>.End(result, out newCorrelationState);
                }
            }

            public override void OnAbort()
            {
                this.outgoingProtocol.Close(true, TimeSpan.Zero);
                this.incomingProtocol.Close(true, TimeSpan.Zero);
            }

            public override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.outgoingProtocol.Close(false, helper.RemainingTime());
                this.incomingProtocol.Close(false, helper.RemainingTime());
            }

            public override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.outgoingProtocol.Open(helper.RemainingTime());
                this.incomingProtocol.Open(helper.RemainingTime());
            }

            public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
            {
                if (this.outgoingProtocol != null)
                {
                    this.outgoingProtocol.SecureOutgoingMessage(ref message, timeout);
                }
            }

            public override SecurityProtocolCorrelationState SecureOutgoingMessage(ref Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState)
            {
                if (this.outgoingProtocol != null)
                {
                    return this.outgoingProtocol.SecureOutgoingMessage(ref message, timeout, correlationState);
                }
                return null;
            }

            public override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                if (this.incomingProtocol != null)
                {
                    this.incomingProtocol.VerifyIncomingMessage(ref message, timeout);
                }
            }

            public override SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationStates)
            {
                if (this.incomingProtocol != null)
                {
                    return this.incomingProtocol.VerifyIncomingMessage(ref message, timeout, correlationStates);
                }
                return null;
            }
        }
    }
}

