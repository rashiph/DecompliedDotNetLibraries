namespace System.Configuration
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;

    public class ProtectedConfigurationProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is ProtectedConfigurationProvider))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_provider_must_implement_type", new object[] { typeof(ProtectedConfigurationProvider).ToString() }), "provider");
            }
            base.Add(provider);
        }

        public ProtectedConfigurationProvider this[string name]
        {
            get
            {
                return (ProtectedConfigurationProvider) base[name];
            }
        }
    }
}

