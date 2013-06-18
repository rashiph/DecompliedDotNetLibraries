namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Tokens;

    public abstract class UserNameSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        protected UserNameSecurityTokenAuthenticator()
        {
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return (token is UserNameSecurityToken);
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            UserNameSecurityToken token2 = (UserNameSecurityToken) token;
            return this.ValidateUserNamePasswordCore(token2.UserName, token2.Password);
        }

        protected abstract ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password);
    }
}

