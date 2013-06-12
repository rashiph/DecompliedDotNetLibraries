namespace System.Configuration
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class SettingsProviderAttribute : Attribute
    {
        private readonly string _providerTypeName;

        public SettingsProviderAttribute(string providerTypeName)
        {
            this._providerTypeName = providerTypeName;
        }

        public SettingsProviderAttribute(Type providerType)
        {
            if (providerType != null)
            {
                this._providerTypeName = providerType.AssemblyQualifiedName;
            }
        }

        public string ProviderTypeName
        {
            get
            {
                return this._providerTypeName;
            }
        }
    }
}

