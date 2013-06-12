namespace System.Configuration
{
    using System;
    using System.Configuration.Provider;
    using System.Reflection;

    public class SettingsProviderCollection : ProviderCollection
    {
        public override void Add(ProviderBase provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!(provider is SettingsProvider))
            {
                throw new ArgumentException(System.SR.GetString("Config_provider_must_implement_type", new object[] { typeof(SettingsProvider).ToString() }), "provider");
            }
            base.Add(provider);
        }

        public SettingsProvider this[string name]
        {
            get
            {
                return (SettingsProvider) base[name];
            }
        }
    }
}

