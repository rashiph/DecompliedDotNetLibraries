namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Principal;

    public class KerberosSecurityTokenProvider : SecurityTokenProvider
    {
        private System.Net.NetworkCredential networkCredential;
        private string servicePrincipalName;
        private System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel;

        public KerberosSecurityTokenProvider(string servicePrincipalName) : this(servicePrincipalName, System.Security.Principal.TokenImpersonationLevel.Identification)
        {
        }

        public KerberosSecurityTokenProvider(string servicePrincipalName, System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel) : this(servicePrincipalName, tokenImpersonationLevel, null)
        {
        }

        public KerberosSecurityTokenProvider(string servicePrincipalName, System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel, System.Net.NetworkCredential networkCredential)
        {
            if (servicePrincipalName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("servicePrincipalName");
            }
            if ((tokenImpersonationLevel != System.Security.Principal.TokenImpersonationLevel.Identification) && (tokenImpersonationLevel != System.Security.Principal.TokenImpersonationLevel.Impersonation))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("tokenImpersonationLevel", System.IdentityModel.SR.GetString("ImpersonationLevelNotSupported", new object[] { tokenImpersonationLevel })));
            }
            this.servicePrincipalName = servicePrincipalName;
            this.tokenImpersonationLevel = tokenImpersonationLevel;
            this.networkCredential = networkCredential;
        }

        internal SecurityToken GetToken(TimeSpan timeout, ChannelBinding channelbinding)
        {
            return new KerberosRequestorSecurityToken(this.ServicePrincipalName, this.TokenImpersonationLevel, this.NetworkCredential, SecurityUniqueId.Create().Value, channelbinding);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.GetToken(timeout, null);
        }

        public System.Net.NetworkCredential NetworkCredential
        {
            get
            {
                return this.networkCredential;
            }
        }

        public string ServicePrincipalName
        {
            get
            {
                return this.servicePrincipalName;
            }
        }

        public System.Security.Principal.TokenImpersonationLevel TokenImpersonationLevel
        {
            get
            {
                return this.tokenImpersonationLevel;
            }
        }
    }
}

