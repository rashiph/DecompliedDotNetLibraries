namespace System.ServiceModel.Security
{
    using System;
    using System.IdentityModel.Selectors;

    public abstract class SecurityCredentialsManager
    {
        protected SecurityCredentialsManager()
        {
        }

        public abstract SecurityTokenManager CreateSecurityTokenManager();
    }
}

