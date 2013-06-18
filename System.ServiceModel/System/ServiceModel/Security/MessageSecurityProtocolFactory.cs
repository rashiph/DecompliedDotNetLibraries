namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security.Tokens;

    internal abstract class MessageSecurityProtocolFactory : SecurityProtocolFactory
    {
        private bool applyConfidentiality;
        private bool applyIntegrity;
        internal const bool defaultDoRequestSignatureConfirmation = false;
        internal const System.ServiceModel.Security.MessageProtectionOrder defaultMessageProtectionOrder = System.ServiceModel.Security.MessageProtectionOrder.SignBeforeEncrypt;
        private bool doRequestSignatureConfirmation;
        private System.ServiceModel.Security.IdentityVerifier identityVerifier;
        private System.ServiceModel.Security.MessageProtectionOrder messageProtectionOrder;
        private ChannelProtectionRequirements protectionRequirements;
        private bool requireConfidentiality;
        private bool requireIntegrity;
        private List<SecurityTokenAuthenticator> wrappedKeyTokenAuthenticator;

        protected MessageSecurityProtocolFactory()
        {
            this.applyIntegrity = true;
            this.applyConfidentiality = true;
            this.protectionRequirements = new ChannelProtectionRequirements();
            this.requireIntegrity = true;
            this.requireConfidentiality = true;
        }

        internal MessageSecurityProtocolFactory(MessageSecurityProtocolFactory factory) : base(factory)
        {
            this.applyIntegrity = true;
            this.applyConfidentiality = true;
            this.protectionRequirements = new ChannelProtectionRequirements();
            this.requireIntegrity = true;
            this.requireConfidentiality = true;
            if (factory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("factory");
            }
            this.applyIntegrity = factory.applyIntegrity;
            this.applyConfidentiality = factory.applyConfidentiality;
            this.identityVerifier = factory.identityVerifier;
            this.protectionRequirements = new ChannelProtectionRequirements(factory.protectionRequirements);
            this.messageProtectionOrder = factory.messageProtectionOrder;
            this.requireIntegrity = factory.requireIntegrity;
            this.requireConfidentiality = factory.requireConfidentiality;
            this.doRequestSignatureConfirmation = factory.doRequestSignatureConfirmation;
        }

        private static MessagePartSpecification ExtractMessageParts(string action, ScopedMessagePartSpecification scopedParts, bool isForSignature)
        {
            MessagePartSpecification parts = null;
            if (scopedParts.TryGetParts(action, out parts))
            {
                return parts;
            }
            if (scopedParts.TryGetParts("*", out parts))
            {
                return parts;
            }
            SecurityVersion securityVersion = MessageSecurityVersion.Default.SecurityVersion;
            FaultCode subCode = new FaultCode(securityVersion.InvalidSecurityFaultCode.Value, securityVersion.HeaderNamespace.Value);
            FaultCode code = FaultCode.CreateSenderFaultCode(subCode);
            FaultReason reason = new FaultReason(System.ServiceModel.SR.GetString("InvalidOrUnrecognizedAction", new object[] { action }), CultureInfo.CurrentCulture);
            MessageFault fault = MessageFault.CreateFault(code, reason);
            if (isForSignature)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoSignaturePartsSpecified", new object[] { action }), null, fault));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("NoEncryptionPartsSpecified", new object[] { action }), null, fault));
        }

        internal MessagePartSpecification GetIncomingEncryptionParts(string action)
        {
            if (!this.RequireConfidentiality)
            {
                return MessagePartSpecification.NoParts;
            }
            if (base.IsDuplexReply)
            {
                return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingEncryptionParts, false);
            }
            return ExtractMessageParts(action, base.ActAsInitiator ? this.ProtectionRequirements.OutgoingEncryptionParts : this.ProtectionRequirements.IncomingEncryptionParts, false);
        }

        internal MessagePartSpecification GetIncomingSignatureParts(string action)
        {
            if (!this.RequireIntegrity)
            {
                return MessagePartSpecification.NoParts;
            }
            if (base.IsDuplexReply)
            {
                return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingSignatureParts, true);
            }
            return ExtractMessageParts(action, base.ActAsInitiator ? this.ProtectionRequirements.OutgoingSignatureParts : this.ProtectionRequirements.IncomingSignatureParts, true);
        }

        internal MessagePartSpecification GetOutgoingEncryptionParts(string action)
        {
            if (!this.ApplyConfidentiality)
            {
                return MessagePartSpecification.NoParts;
            }
            if (base.IsDuplexReply)
            {
                return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingEncryptionParts, false);
            }
            return ExtractMessageParts(action, base.ActAsInitiator ? this.ProtectionRequirements.IncomingEncryptionParts : this.ProtectionRequirements.OutgoingEncryptionParts, false);
        }

        internal MessagePartSpecification GetOutgoingSignatureParts(string action)
        {
            if (!this.ApplyIntegrity)
            {
                return MessagePartSpecification.NoParts;
            }
            if (base.IsDuplexReply)
            {
                return ExtractMessageParts(action, this.ProtectionRequirements.OutgoingSignatureParts, true);
            }
            return ExtractMessageParts(action, base.ActAsInitiator ? this.ProtectionRequirements.IncomingSignatureParts : this.ProtectionRequirements.OutgoingSignatureParts, true);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            this.protectionRequirements.MakeReadOnly();
            if (base.DetectReplays && !this.RequireIntegrity)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("RequireIntegrity", System.ServiceModel.SR.GetString("ForReplayDetectionToBeDoneRequireIntegrityMustBeSet"));
            }
            if (this.DoRequestSignatureConfirmation)
            {
                if (!this.SupportsRequestReply)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SignatureConfirmationRequiresRequestReply"));
                }
                if (!base.StandardsManager.SecurityVersion.SupportsSignatureConfirmation)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SecurityVersionDoesNotSupportSignatureConfirmation", new object[] { base.StandardsManager.SecurityVersion }));
                }
            }
            this.wrappedKeyTokenAuthenticator = new List<SecurityTokenAuthenticator>(1);
            SecurityTokenAuthenticator item = new NonValidatingSecurityTokenAuthenticator<WrappedKeySecurityToken>();
            this.wrappedKeyTokenAuthenticator.Add(item);
            this.ValidateCorrelationSecuritySettings();
        }

        protected virtual void ValidateCorrelationSecuritySettings()
        {
            if (base.ActAsInitiator && this.SupportsRequestReply)
            {
                bool flag = this.ApplyIntegrity || this.ApplyConfidentiality;
                bool flag2 = this.RequireIntegrity || this.RequireConfidentiality;
                if (!flag && flag2)
                {
                    base.OnPropertySettingsError("ApplyIntegrity", false);
                }
            }
        }

        public bool ApplyConfidentiality
        {
            get
            {
                return this.applyConfidentiality;
            }
            set
            {
                base.ThrowIfImmutable();
                this.applyConfidentiality = value;
            }
        }

        public bool ApplyIntegrity
        {
            get
            {
                return this.applyIntegrity;
            }
            set
            {
                base.ThrowIfImmutable();
                this.applyIntegrity = value;
            }
        }

        public bool DoRequestSignatureConfirmation
        {
            get
            {
                return this.doRequestSignatureConfirmation;
            }
            set
            {
                base.ThrowIfImmutable();
                this.doRequestSignatureConfirmation = value;
            }
        }

        public System.ServiceModel.Security.IdentityVerifier IdentityVerifier
        {
            get
            {
                return this.identityVerifier;
            }
            set
            {
                base.ThrowIfImmutable();
                this.identityVerifier = value;
            }
        }

        public System.ServiceModel.Security.MessageProtectionOrder MessageProtectionOrder
        {
            get
            {
                return this.messageProtectionOrder;
            }
            set
            {
                base.ThrowIfImmutable();
                this.messageProtectionOrder = value;
            }
        }

        public ChannelProtectionRequirements ProtectionRequirements
        {
            get
            {
                return this.protectionRequirements;
            }
        }

        public bool RequireConfidentiality
        {
            get
            {
                return this.requireConfidentiality;
            }
            set
            {
                base.ThrowIfImmutable();
                this.requireConfidentiality = value;
            }
        }

        public bool RequireIntegrity
        {
            get
            {
                return this.requireIntegrity;
            }
            set
            {
                base.ThrowIfImmutable();
                this.requireIntegrity = value;
            }
        }

        internal List<SecurityTokenAuthenticator> WrappedKeySecurityTokenAuthenticator
        {
            get
            {
                return this.wrappedKeyTokenAuthenticator;
            }
        }
    }
}

