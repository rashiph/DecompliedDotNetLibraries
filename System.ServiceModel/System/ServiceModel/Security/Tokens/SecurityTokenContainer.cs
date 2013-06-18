namespace System.ServiceModel.Security.Tokens
{
    using System;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;

    internal class SecurityTokenContainer
    {
        private SecurityToken token;

        public SecurityTokenContainer(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            this.token = token;
        }

        public SecurityToken Token
        {
            get
            {
                return this.token;
            }
        }
    }
}

