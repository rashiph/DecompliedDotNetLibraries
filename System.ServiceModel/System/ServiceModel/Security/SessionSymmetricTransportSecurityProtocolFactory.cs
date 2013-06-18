namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SessionSymmetricTransportSecurityProtocolFactory : TransportSecurityProtocolFactory
    {
        private SessionDerivedKeySecurityTokenParameters derivedKeyTokenParameters;
        private System.ServiceModel.Security.Tokens.SecurityTokenParameters securityTokenParameters;

        internal System.ServiceModel.Security.Tokens.SecurityTokenParameters GetTokenParameters()
        {
            if (this.derivedKeyTokenParameters != null)
            {
                return this.derivedKeyTokenParameters;
            }
            return this.securityTokenParameters;
        }

        protected override SecurityProtocol OnCreateSecurityProtocol(EndpointAddress target, Uri via, object listenerSecurityState, TimeSpan timeout)
        {
            if (base.ActAsInitiator)
            {
                return new InitiatorSessionSymmetricTransportSecurityProtocol(this, target, via);
            }
            return new AcceptorSessionSymmetricTransportSecurityProtocol(this);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            base.OnOpen(timeout);
            if (this.SecurityTokenParameters == null)
            {
                base.OnPropertySettingsError("SecurityTokenParameters", true);
            }
            if (this.SecurityTokenParameters.RequireDerivedKeys)
            {
                base.ExpectKeyDerivation = true;
                this.derivedKeyTokenParameters = new SessionDerivedKeySecurityTokenParameters(base.ActAsInitiator);
            }
        }

        public System.ServiceModel.Security.Tokens.SecurityTokenParameters SecurityTokenParameters
        {
            get
            {
                return this.securityTokenParameters;
            }
            set
            {
                base.ThrowIfImmutable();
                this.securityTokenParameters = value;
            }
        }

        public override bool SupportsReplayDetection
        {
            get
            {
                return true;
            }
        }
    }
}

