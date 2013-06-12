namespace System.Web.Profile
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ProfileProviderAttribute : Attribute
    {
        private string _ProviderName;

        public ProfileProviderAttribute(string providerName)
        {
            this._ProviderName = providerName;
        }

        public string ProviderName
        {
            get
            {
                return this._ProviderName;
            }
        }
    }
}

