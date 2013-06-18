namespace System.IdentityModel.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel;

    public abstract class AuthorizationContext : IAuthorizationComponent
    {
        protected AuthorizationContext()
        {
        }

        public static AuthorizationContext CreateDefaultAuthorizationContext(IList<IAuthorizationPolicy> authorizationPolicies)
        {
            return System.IdentityModel.SecurityUtils.CreateDefaultAuthorizationContext(authorizationPolicies);
        }

        public abstract ReadOnlyCollection<ClaimSet> ClaimSets { get; }

        public abstract DateTime ExpirationTime { get; }

        public abstract string Id { get; }

        public abstract IDictionary<string, object> Properties { get; }
    }
}

