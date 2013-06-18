namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.Xml;

    public class SecurityContextSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is SecurityContextSecurityToken);
        }

        private bool IsTimeValid(SecurityContextSecurityToken sct)
        {
            DateTime utcNow = DateTime.UtcNow;
            return (((sct.ValidFrom <= utcNow) && (sct.ValidTo >= utcNow)) && (sct.KeyEffectiveTime <= utcNow));
        }

        private void ThrowExpiredContextFaultException(UniqueId contextId, SecurityContextSecurityToken sct)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new SecurityContextTokenValidationException(System.ServiceModel.SR.GetString("SecurityContextExpired", new object[] { contextId, (sct.KeyGeneration == null) ? "none" : sct.KeyGeneration.ToString() })));
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            SecurityContextSecurityToken sct = (SecurityContextSecurityToken) token;
            if (!this.IsTimeValid(sct))
            {
                this.ThrowExpiredContextFaultException(sct.ContextId, sct);
            }
            return sct.AuthorizationPolicies;
        }
    }
}

