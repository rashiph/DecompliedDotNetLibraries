namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;

    public abstract class SecurityTokenAuthenticator
    {
        protected SecurityTokenAuthenticator()
        {
        }

        public bool CanValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            return this.CanValidateTokenCore(token);
        }

        protected abstract bool CanValidateTokenCore(SecurityToken token);
        public ReadOnlyCollection<IAuthorizationPolicy> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (!this.CanValidateToken(token))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("CannotValidateSecurityTokenType", new object[] { this, token.GetType() })));
            }
            ReadOnlyCollection<IAuthorizationPolicy> onlys = this.ValidateTokenCore(token);
            if (onlys == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(System.IdentityModel.SR.GetString("CannotValidateSecurityTokenType", new object[] { this, token.GetType() })));
            }
            return onlys;
        }

        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token);
    }
}

