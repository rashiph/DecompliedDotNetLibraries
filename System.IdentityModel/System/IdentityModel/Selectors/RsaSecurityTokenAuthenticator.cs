namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;

    public class RsaSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is RsaSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            RsaSecurityToken token2 = (RsaSecurityToken) token;
            List<Claim> claims = new List<Claim>(2) {
                new Claim(ClaimTypes.Rsa, token2.Rsa, Rights.Identity),
                Claim.CreateRsaClaim(token2.Rsa)
            };
            DefaultClaimSet issuance = new DefaultClaimSet(ClaimSet.Anonymous, claims);
            return new List<IAuthorizationPolicy>(1) { new UnconditionalPolicy(issuance, token2.ValidTo) }.AsReadOnly();
        }
    }
}

