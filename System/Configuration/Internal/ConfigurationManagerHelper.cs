namespace System.Configuration.Internal
{
    using System;
    using System.Net.Configuration;

    internal sealed class ConfigurationManagerHelper : IConfigurationManagerHelper
    {
        private ConfigurationManagerHelper()
        {
        }

        void IConfigurationManagerHelper.EnsureNetConfigLoaded()
        {
            SettingsSection.EnsureConfigLoaded();
        }
    }
}

