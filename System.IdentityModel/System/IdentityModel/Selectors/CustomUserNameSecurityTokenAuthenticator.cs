namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Principal;

    public class CustomUserNameSecurityTokenAuthenticator : UserNameSecurityTokenAuthenticator
    {
        private UserNamePasswordValidator validator;

        public CustomUserNameSecurityTokenAuthenticator(UserNamePasswordValidator validator)
        {
            if (validator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("validator");
            }
            this.validator = validator;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            this.validator.Validate(userName, password);
            return System.IdentityModel.SecurityUtils.CreateAuthorizationPolicies(new UserNameClaimSet(userName, this.validator.GetType().Name));
        }

        private class UserNameClaimSet : DefaultClaimSet, IIdentityInfo
        {
            private IIdentity identity;

            public UserNameClaimSet(string userName, string authType) : base(new Claim[0])
            {
                if (userName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
                }
                this.identity = System.IdentityModel.SecurityUtils.CreateIdentity(userName, authType);
                List<Claim> claims = new List<Claim>(2) {
                    new Claim(ClaimTypes.Name, userName, Rights.Identity),
                    Claim.CreateNameClaim(userName)
                };
                base.Initialize(ClaimSet.System, claims);
            }

            public IIdentity Identity
            {
                get
                {
                    return this.identity;
                }
            }
        }
    }
}

