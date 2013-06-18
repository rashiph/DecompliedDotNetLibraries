namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security;

    internal class KerberosRequestorSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is KerberosRequestorSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            KerberosRequestorSecurityToken token2 = (KerberosRequestorSecurityToken) token;
            List<IAuthorizationPolicy> list = new List<IAuthorizationPolicy>(1);
            ClaimSet issuance = new DefaultClaimSet(ClaimSet.System, new Claim[] { new Claim(ClaimTypes.Spn, token2.ServicePrincipalName, Rights.PossessProperty) });
            list.Add(new UnconditionalPolicy(System.ServiceModel.Security.SecurityUtils.CreateIdentity(token2.ServicePrincipalName, "Kerberos"), issuance));
            return list.AsReadOnly();
        }
    }
}

