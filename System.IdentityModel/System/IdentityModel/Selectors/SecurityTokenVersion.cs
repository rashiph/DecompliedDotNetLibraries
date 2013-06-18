namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.ObjectModel;

    public abstract class SecurityTokenVersion
    {
        protected SecurityTokenVersion()
        {
        }

        public abstract ReadOnlyCollection<string> GetSecuritySpecifications();
    }
}

