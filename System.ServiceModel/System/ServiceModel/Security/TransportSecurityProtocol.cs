namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal class TransportSecurityProtocol : SecurityProtocol
    {
        public TransportSecurityProtocol(TransportSecurityProtocolFactory factory, EndpointAddress target, Uri via) : base(factory, target, via)
        {
        }

        protected void AttachRecipientSecurityProperty(Message message, IList<SecurityToken> basicTokens, IList<SecurityToken> endorsingTokens, IList<SecurityToken> signedEndorsingTokens, IList<SecurityToken> signedTokens, Dictionary<SecurityToken, ReadOnlyCollection<IAuthorizationPolicy>> tokenPoliciesMapping)
        {
            SecurityMessageProperty orCreate = SecurityMessageProperty.GetOrCreate(message);
            base.AddSupportingTokenSpecification(orCreate, basicTokens, endorsingTokens, signedEndorsingTokens, signedTokens, tokenPoliciesMapping);
            orCreate.ServiceSecurityContext = new ServiceSecurityContext(orCreate.GetInitiatorTokenAuthorizationPolicies());
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginSecureOutgoingMessage(message, timeout, null, callback, state);
        }

        public override IAsyncResult BeginSecureOutgoingMessage(Message message, TimeSpan timeout, SecurityProtocolCorrelationState correlationState, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            string actor = string.Empty;
            try
            {
                if (base.SecurityProtocolFactory.ActAsInitiator)
                {
                    return this.BeginSecureOutgoingMessageAtInitiatorCore(message, actor, timeout, callback, state);
                }
                this.SecureOutgoingMessageAtResponder(ref message, actor);
                result = new CompletedAsyncResult<Message>(message, callback, state);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
            return result;
        }

        protected virtual IAsyncResult BeginSecureOutgoingMessageAtInitiatorCore(Message message, string actor, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IList<SupportingTokenSpecification> list;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (base.TryGetSupportingTokens(base.SecurityProtocolFactory, base.Target, base.Via, message, helper.RemainingTime(), false, out list))
            {
                this.SetUpDelayedSecurityExecution(ref message, actor, list);
                return new CompletedAsyncResult<Message>(message, callback, state);
            }
            return new SecureOutgoingMessageAsyncResult(actor, message, this, timeout, callback, state);
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message)
        {
            SecurityProtocolCorrelationState state;
            this.EndSecureOutgoingMessage(result, out message, out state);
        }

        public override void EndSecureOutgoingMessage(IAsyncResult result, out Message message, out SecurityProtocolCorrelationState newCorrelationState)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            newCorrelationState = null;
            try
            {
                if (result is CompletedAsyncResult<Message>)
                {
                    message = CompletedAsyncResult<Message>.End(result);
                }
                else
                {
                    message = this.EndSecureOutgoingMessageAtInitiatorCore(result);
                }
                base.OnOutgoingMessageSecured(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.OnSecureOutgoingMessageFailure(null);
                throw;
            }
        }

        protected virtual Message EndSecureOutgoingMessageAtInitiatorCore(IAsyncResult result)
        {
            if (result is CompletedAsyncResult<Message>)
            {
                return CompletedAsyncResult<Message>.End(result);
            }
            return SecureOutgoingMessageAsyncResult.End(result);
        }

        public override void SecureOutgoingMessage(ref Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            string actor = string.Empty;
            try
            {
                if (base.SecurityProtocolFactory.ActAsInitiator)
                {
                    this.SecureOutgoingMessageAtInitiator(ref message, actor, timeout);
                }
                else
                {
                    this.SecureOutgoingMessageAtResponder(ref message, actor);
                }
                base.OnOutgoingMessageSecured(message);
            }
            catch
            {
                base.OnSecureOutgoingMessageFailure(message);
                throw;
            }
        }

        protected virtual void SecureOutgoingMessageAtInitiator(ref Message message, string actor, TimeSpan timeout)
        {
            IList<SupportingTokenSpecification> list;
            base.TryGetSupportingTokens(base.SecurityProtocolFactory, base.Target, base.Via, message, timeout, true, out list);
            this.SetUpDelayedSecurityExecution(ref message, actor, list);
        }

        protected void SecureOutgoingMessageAtResponder(ref Message message, string actor)
        {
            if (base.SecurityProtocolFactory.AddTimestamp && !base.SecurityProtocolFactory.SecurityBindingElement.EnableUnsecuredResponse)
            {
                message = base.CreateSendSecurityHeaderForTransportProtocol(message, actor, base.SecurityProtocolFactory).SetupExecution();
            }
        }

        internal void SetUpDelayedSecurityExecution(ref Message message, string actor, IList<SupportingTokenSpecification> supportingTokens)
        {
            SendSecurityHeader securityHeader = base.CreateSendSecurityHeaderForTransportProtocol(message, actor, base.SecurityProtocolFactory);
            base.AddSupportingTokens(securityHeader, supportingTokens);
            message = securityHeader.SetupExecution();
        }

        public sealed override void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            base.CommunicationObject.ThrowIfClosedOrNotOpen();
            try
            {
                this.VerifyIncomingMessageCore(ref message, timeout);
            }
            catch (MessageSecurityException exception)
            {
                base.OnVerifyIncomingMessageFailure(message, exception);
                throw;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                base.OnVerifyIncomingMessageFailure(message, exception2);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("MessageSecurityVerificationFailed"), exception2));
            }
        }

        protected virtual void VerifyIncomingMessageCore(ref Message message, TimeSpan timeout)
        {
            bool flag;
            bool flag2;
            bool flag3;
            TransportSecurityProtocolFactory securityProtocolFactory = (TransportSecurityProtocolFactory) base.SecurityProtocolFactory;
            string actor = string.Empty;
            ReceiveSecurityHeader header = securityProtocolFactory.StandardsManager.TryCreateReceiveSecurityHeader(message, actor, securityProtocolFactory.IncomingAlgorithmSuite, securityProtocolFactory.ActAsInitiator ? MessageDirection.Output : MessageDirection.Input);
            IList<SupportingTokenAuthenticatorSpecification> supportingTokenAuthenticators = securityProtocolFactory.GetSupportingTokenAuthenticators(message.Headers.Action, out flag3, out flag, out flag2);
            if (header == null)
            {
                bool flag4 = (flag2 || flag3) || flag;
                if ((securityProtocolFactory.ActAsInitiator && (!securityProtocolFactory.AddTimestamp || securityProtocolFactory.SecurityBindingElement.EnableUnsecuredResponse)) || ((!securityProtocolFactory.ActAsInitiator && !securityProtocolFactory.AddTimestamp) && !flag4))
                {
                    return;
                }
                if (string.IsNullOrEmpty(actor))
                {
                    throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindSecurityHeaderInMessageNoActor")), message);
                }
                throw TraceUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("UnableToFindSecurityHeaderInMessage", new object[] { actor })), message);
            }
            header.RequireMessageProtection = false;
            header.ExpectBasicTokens = flag;
            header.ExpectSignedTokens = flag3;
            header.ExpectEndorsingTokens = flag2;
            header.MaxReceivedMessageSize = securityProtocolFactory.SecurityBindingElement.MaxReceivedMessageSize;
            header.ReaderQuotas = securityProtocolFactory.SecurityBindingElement.ReaderQuotas;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!securityProtocolFactory.ActAsInitiator)
            {
                header.ConfigureTransportBindingServerReceiveHeader(supportingTokenAuthenticators);
                header.ConfigureOutOfBandTokenResolver(base.MergeOutOfBandResolvers(supportingTokenAuthenticators, EmptyReadOnlyCollection<SecurityTokenResolver>.Instance));
                if (securityProtocolFactory.ExpectKeyDerivation)
                {
                    header.DerivedTokenAuthenticator = securityProtocolFactory.DerivedKeyTokenAuthenticator;
                }
            }
            header.SetTimeParameters(securityProtocolFactory.NonceCache, securityProtocolFactory.ReplayWindow, securityProtocolFactory.MaxClockSkew);
            header.Process(helper.RemainingTime(), System.ServiceModel.Security.SecurityUtils.GetChannelBindingFromMessage(message), securityProtocolFactory.ExtendedProtectionPolicy);
            message = header.ProcessedMessage;
            if (!securityProtocolFactory.ActAsInitiator)
            {
                this.AttachRecipientSecurityProperty(message, header.BasicSupportingTokens, header.EndorsingSupportingTokens, header.SignedEndorsingSupportingTokens, header.SignedSupportingTokens, header.SecurityTokenAuthorizationPoliciesMapping);
            }
            base.OnIncomingMessageVerified(message);
        }

        private sealed class SecureOutgoingMessageAsyncResult : SecurityProtocol.GetSupportingTokensAsyncResult
        {
            private string actor;
            private TransportSecurityProtocol binding;
            private Message message;

            public SecureOutgoingMessageAsyncResult(string actor, Message message, TransportSecurityProtocol binding, TimeSpan timeout, AsyncCallback callback, object state) : base(message, binding, timeout, callback, state)
            {
                this.actor = actor;
                this.message = message;
                this.binding = binding;
                base.Start();
            }

            internal static Message End(IAsyncResult result)
            {
                return AsyncResult.End<TransportSecurityProtocol.SecureOutgoingMessageAsyncResult>(result).message;
            }

            protected override bool OnGetSupportingTokensDone(TimeSpan timeout)
            {
                this.binding.SetUpDelayedSecurityExecution(ref this.message, this.actor, base.SupportingTokens);
                return true;
            }
        }
    }
}

