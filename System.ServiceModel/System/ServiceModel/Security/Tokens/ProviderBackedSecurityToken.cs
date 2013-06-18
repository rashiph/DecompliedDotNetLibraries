namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Runtime;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;

    internal class ProviderBackedSecurityToken : SecurityToken
    {
        private System.Security.Authentication.ExtendedProtection.ChannelBinding _channelBinding;
        private object _lock = new object();
        private SecurityToken _securityToken;
        private TimeSpan _timeout;
        private SecurityTokenProvider _tokenProvider;

        public ProviderBackedSecurityToken(SecurityTokenProvider tokenProvider, TimeSpan timeout)
        {
            if (tokenProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("tokenProvider"));
            }
            this._tokenProvider = tokenProvider;
            this._timeout = timeout;
        }

        private void ResolveSecurityToken()
        {
            if (this._securityToken == null)
            {
                lock (this._lock)
                {
                    if (this._securityToken == null)
                    {
                        ClientCredentialsSecurityTokenManager.KerberosSecurityTokenProviderWrapper wrapper = this._tokenProvider as ClientCredentialsSecurityTokenManager.KerberosSecurityTokenProviderWrapper;
                        if (wrapper != null)
                        {
                            this._securityToken = wrapper.GetToken(new TimeoutHelper(this._timeout).RemainingTime(), this._channelBinding);
                        }
                        else
                        {
                            this._securityToken = this._tokenProvider.GetToken(new TimeoutHelper(this._timeout).RemainingTime());
                        }
                    }
                }
            }
            if (this._securityToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(System.ServiceModel.SR.GetString("SecurityTokenNotResolved", new object[] { this._tokenProvider.GetType().ToString() })));
            }
        }

        public System.Security.Authentication.ExtendedProtection.ChannelBinding ChannelBinding
        {
            set
            {
                this._channelBinding = value;
            }
        }

        public override string Id
        {
            get
            {
                if (this._securityToken == null)
                {
                    this.ResolveSecurityToken();
                }
                return this._securityToken.Id;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this._securityToken == null)
                {
                    this.ResolveSecurityToken();
                }
                return this._securityToken.SecurityKeys;
            }
        }

        public SecurityToken Token
        {
            get
            {
                if (this._securityToken == null)
                {
                    this.ResolveSecurityToken();
                }
                return this._securityToken;
            }
        }

        public SecurityTokenProvider TokenProvider
        {
            get
            {
                return this._tokenProvider;
            }
        }

        public override DateTime ValidFrom
        {
            get
            {
                if (this._securityToken == null)
                {
                    this.ResolveSecurityToken();
                }
                return this._securityToken.ValidFrom;
            }
        }

        public override DateTime ValidTo
        {
            get
            {
                if (this._securityToken == null)
                {
                    this.ResolveSecurityToken();
                }
                return this._securityToken.ValidTo;
            }
        }
    }
}

