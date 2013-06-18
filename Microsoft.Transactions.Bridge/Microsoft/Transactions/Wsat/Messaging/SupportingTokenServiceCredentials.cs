namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;

    internal class SupportingTokenServiceCredentials : ServiceCredentials
    {
        private SecurityContextSecurityTokenAuthenticator tokenAuthenticator;
        private SupportingTokenSecurityTokenResolver tokenResolver;

        public SupportingTokenServiceCredentials()
        {
            this.tokenResolver = new SupportingTokenSecurityTokenResolver();
            this.tokenAuthenticator = new SecurityContextSecurityTokenAuthenticator();
        }

        private SupportingTokenServiceCredentials(SupportingTokenServiceCredentials other) : base(other)
        {
            this.tokenResolver = other.tokenResolver;
            this.tokenAuthenticator = other.tokenAuthenticator;
        }

        protected override ServiceCredentials CloneCore()
        {
            return new SupportingTokenServiceCredentials(this);
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new SupportingTokenSecurityTokenManager(this);
        }

        public SupportingTokenSecurityTokenResolver TokenResolver
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.tokenResolver;
            }
        }

        private class SupportingTokenSecurityTokenManager : ServiceCredentialsSecurityTokenManager
        {
            private SupportingTokenServiceCredentials serverCreds;

            public SupportingTokenSecurityTokenManager(SupportingTokenServiceCredentials serverCreds) : base(serverCreds)
            {
                this.serverCreds = serverCreds;
            }

            public override SecurityTokenAuthenticator CreateSecurityTokenAuthenticator(SecurityTokenRequirement tokenRequirement, out SecurityTokenResolver outOfBandTokenResolver)
            {
                if (tokenRequirement == null)
                {
                    throw Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenRequirement");
                }
                if (!(tokenRequirement.TokenType == ServiceModelSecurityTokenTypes.SecurityContext))
                {
                    return base.CreateSecurityTokenAuthenticator(tokenRequirement, out outOfBandTokenResolver);
                }
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "CreateSecurityTokenAuthenticator for SecurityContext");
                }
                outOfBandTokenResolver = this.serverCreds.tokenResolver;
                return this.serverCreds.tokenAuthenticator;
            }

            public override EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement)
            {
                return null;
            }
        }
    }
}

