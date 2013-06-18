namespace System.IdentityModel.Selectors
{
    using System;
    using System.IdentityModel;
    using System.IdentityModel.Tokens;

    public class UserNameSecurityTokenProvider : SecurityTokenProvider
    {
        private UserNameSecurityToken userNameToken;

        public UserNameSecurityTokenProvider(string userName, string password)
        {
            if (userName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("userName");
            }
            this.userNameToken = new UserNameSecurityToken(userName, password);
        }

        protected override SecurityToken GetTokenCore(TimeSpan timeout)
        {
            return this.userNameToken;
        }
    }
}

