namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    public class SecurityTokenSpecification
    {
        private System.IdentityModel.Tokens.SecurityToken token;
        private ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies;

        public SecurityTokenSpecification(System.IdentityModel.Tokens.SecurityToken token, ReadOnlyCollection<IAuthorizationPolicy> tokenPolicies)
        {
            this.token = token;
            if (tokenPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenPolicies");
            }
            this.tokenPolicies = tokenPolicies;
        }

        public System.IdentityModel.Tokens.SecurityToken SecurityToken
        {
            get
            {
                return this.token;
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> SecurityTokenPolicies
        {
            get
            {
                return this.tokenPolicies;
            }
        }
    }
}

