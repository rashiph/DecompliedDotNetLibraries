namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    internal sealed class AcceptorSessionSymmetricTransportSecurityProtocol : TransportSecurityProtocol, IAcceptorSecuritySessionProtocol
    {
        private SecurityToken outgoingSessionToken;
        private bool requireDerivedKeys;
        private UniqueId sessionId;
        private SecurityTokenAuthenticator sessionTokenAuthenticator;
        private Collection<SupportingTokenAuthenticatorSpecification> sessionTokenAuthenticatorSpecificationList;
        private SecurityTokenResolver sessionTokenResolver;
        private ReadOnlyCollection<SecurityTokenResolver> sessionTokenResolverList;

        public AcceptorSessionSymmetricTransportSecurityProtocol(SessionSymmetricTransportSecurityProtocolFactory factory) : base(factory, null, null)
        {
            if (factory.ActAsInitiator)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ProtocolMustBeRecipient", new object[] { base.GetType().ToString() })));
            }
            this.requireDerivedKeys = factory.SecurityTokenParameters.RequireDerivedKeys;
        }

        public SecurityToken GetOutgoingSessionToken()
        {
            return this.outgoingSessionToken;
        }

        public void SetOutgoingSessionToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.outgoingSessionToken = token;
        }

        public void SetSessionTokenAuthenticator(UniqueId sessionId, SecurityTokenAuthenticator sessionTokenAuthenticator, SecurityTokenResolver sessionTokenResolver)
        {
            base.CommunicationObject.ThrowIfDisposedOrImmutable();
            this.sessionId = sessionId;
            this.sessionTokenResolver = sessionTokenResolver;
            Collection<SecurityTokenResolver> list = new Collection<SecurityTokenResolver> {
                this.sessionTokenResolver
            };
            this.sessionTokenResolverList = new ReadOnlyCollection<SecurityTokenResolver>(list);
            this.sessionTokenAuthenticator = sessionTokenAuthenticator;
            SupportingTokenAuthenticatorSpecification item = new SupportingTokenAuthenticatorSpecification(this.sessionTokenAuthenticator, this.sessionTokenResolver, SecurityTokenAttachmentMode.Endorsing, this.Factory.SecurityTokenParameters);
            this.sessionTokenAuthenticatorSpecificationList = new Collection<SupportingTokenAuthenticatorSpecification>();
            this.sessionTokenAuthenticatorSpecificationList.Add(item);
        }

        protected override void VerifyIncomingMessageCore(ref Message message, TimeSpan timeout)
        {
            string actor = string.Empty;
            ReceiveSecurityHeader securityHeader = this.Factory.StandardsManager.CreateReceiveSecurityHeader(message, actor, this.Factory.IncomingAlgorithmSuite, MessageDirection.Input);
            securityHeader.RequireMessageProtection = false;
            securityHeader.ReaderQuotas = this.Factory.SecurityBindingElement.ReaderQuotas;
            IList<SupportingTokenAuthenticatorSpecification> supportingAuthenticators = base.GetSupportingTokenAuthenticatorsAndSetExpectationFlags(this.Factory, message, securityHeader);
            ReadOnlyCollection<SecurityTokenResolver> outOfBandResolvers = base.MergeOutOfBandResolvers(supportingAuthenticators, this.sessionTokenResolverList);
            if ((supportingAuthenticators != null) && (supportingAuthenticators.Count > 0))
            {
                supportingAuthenticators = new List<SupportingTokenAuthenticatorSpecification>(supportingAuthenticators);
                supportingAuthenticators.Insert(0, this.sessionTokenAuthenticatorSpecificationList[0]);
            }
            else
            {
                supportingAuthenticators = this.sessionTokenAuthenticatorSpecificationList;
            }
            securityHeader.ConfigureTransportBindingServerReceiveHeader(supportingAuthenticators);
            securityHeader.ConfigureOutOfBandTokenResolver(outOfBandResolvers);
            securityHeader.ExpectEndorsingTokens = true;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            securityHeader.SetTimeParameters(this.Factory.NonceCache, this.Factory.ReplayWindow, this.Factory.MaxClockSkew);
            securityHeader.EnforceDerivedKeyRequirement = message.Headers.Action != this.Factory.StandardsManager.SecureConversationDriver.CloseAction.Value;
            securityHeader.Process(helper.RemainingTime(), System.ServiceModel.Security.SecurityUtils.GetChannelBindingFromMessage(message), this.Factory.ExtendedProtectionPolicy);
            if (securityHeader.Timestamp == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("RequiredTimestampMissingInSecurityHeader")));
            }
            bool flag = false;
            if (securityHeader.EndorsingSupportingTokens != null)
            {
                for (int i = 0; i < securityHeader.EndorsingSupportingTokens.Count; i++)
                {
                    SecurityContextSecurityToken token = securityHeader.EndorsingSupportingTokens[i] as SecurityContextSecurityToken;
                    if ((token != null) && (token.ContextId == this.sessionId))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSessionTokenPresentInMessage")));
            }
            message = securityHeader.ProcessedMessage;
            base.AttachRecipientSecurityProperty(message, securityHeader.BasicSupportingTokens, securityHeader.EndorsingSupportingTokens, securityHeader.SignedEndorsingSupportingTokens, securityHeader.SignedSupportingTokens, securityHeader.SecurityTokenAuthorizationPoliciesMapping);
            base.OnIncomingMessageVerified(message);
        }

        private SessionSymmetricTransportSecurityProtocolFactory Factory
        {
            get
            {
                return (SessionSymmetricTransportSecurityProtocolFactory) base.SecurityProtocolFactory;
            }
        }

        public bool ReturnCorrelationState
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
    }
}

