namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class SessionSymmetricMessageSecurityProtocolFactory : MessageSecurityProtocolFactory
    {
        private SessionDerivedKeySecurityTokenParameters derivedKeyTokenParameters;
        private System.ServiceModel.Security.Tokens.SecurityTokenParameters securityTokenParameters;

        public override EndpointIdentity GetIdentityOfSelf()
        {
            if (base.SecurityTokenManager is IEndpointIdentityProvider)
            {
                SecurityTokenRequirement requirement = base.CreateRecipientSecurityTokenRequirement();
                this.SecurityTokenParameters.InitializeSecurityTokenRequirement(requirement);
                return ((IEndpointIdentityProvider) base.SecurityTokenManager).GetIdentityOfSelf(requirement);
            }
            return base.GetIdentityOfSelf();
        }

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
                return new InitiatorSessionSymmetricMessageSecurityProtocol(this, target, via);
            }
            return new AcceptorSessionSymmetricMessageSecurityProtocol(this, null);
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.SecurityTokenParameters == null)
            {
                base.OnPropertySettingsError("SecurityTokenParameters", true);
            }
            if (this.SecurityTokenParameters.RequireDerivedKeys)
            {
                base.ExpectKeyDerivation = true;
                this.derivedKeyTokenParameters = new SessionDerivedKeySecurityTokenParameters(base.ActAsInitiator);
            }
            base.OnOpen(timeout);
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
    }
}

