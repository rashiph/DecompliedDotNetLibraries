namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.IdentityModel.Selectors;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    internal class DefaultServiceCredentials : ServiceCredentials
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DefaultServiceCredentials()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DefaultServiceCredentials(DefaultServiceCredentials other) : base(other)
        {
        }

        protected override ServiceCredentials CloneCore()
        {
            return new DefaultServiceCredentials(this);
        }

        public override SecurityTokenManager CreateSecurityTokenManager()
        {
            return new DefaultSecurityTokenManager(this);
        }

        private class DefaultSecurityTokenManager : ServiceCredentialsSecurityTokenManager
        {
            private DefaultServiceCredentials serverCreds;

            public DefaultSecurityTokenManager(DefaultServiceCredentials serverCreds) : base(serverCreds)
            {
                this.serverCreds = serverCreds;
            }

            public override EndpointIdentity GetIdentityOfSelf(SecurityTokenRequirement tokenRequirement)
            {
                return null;
            }
        }
    }
}

