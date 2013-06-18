namespace System.ServiceModel.Security
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    internal class NegotiationTokenAuthenticatorState : IDisposable
    {
        private bool isNegotiationCompleted;
        private SecurityContextSecurityToken serviceToken;
        private object thisLock = new object();

        private void CheckCompleted()
        {
            if (!this.isNegotiationCompleted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("NegotiationIsNotCompleted")));
            }
        }

        public virtual void Dispose()
        {
        }

        public virtual string GetRemoteIdentityName()
        {
            if (this.isNegotiationCompleted)
            {
                return System.ServiceModel.Security.SecurityUtils.GetIdentityNamesFromPolicies(this.serviceToken.AuthorizationPolicies);
            }
            return string.Empty;
        }

        public void SetServiceToken(SecurityContextSecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.serviceToken = token;
            this.isNegotiationCompleted = true;
        }

        public bool IsNegotiationCompleted
        {
            get
            {
                return this.isNegotiationCompleted;
            }
        }

        public SecurityContextSecurityToken ServiceToken
        {
            get
            {
                this.CheckCompleted();
                return this.serviceToken;
            }
        }

        public object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

