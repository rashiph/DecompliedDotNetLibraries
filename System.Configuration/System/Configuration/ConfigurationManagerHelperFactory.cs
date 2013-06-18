namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;

    internal static class ConfigurationManagerHelperFactory
    {
        private const string ConfigurationManagerHelperTypeString = "System.Configuration.Internal.ConfigurationManagerHelper, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private static IConfigurationManagerHelper s_instance;

        internal static IConfigurationManagerHelper Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = (IConfigurationManagerHelper) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission("System.Configuration.Internal.ConfigurationManagerHelper, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                }
                return s_instance;
            }
        }
    }
}

