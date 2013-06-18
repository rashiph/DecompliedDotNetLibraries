namespace System.Configuration.Internal
{
    using System;
    using System.Configuration;

    internal sealed class ConfigurationManagerInternal : IConfigurationManagerInternal
    {
        private ConfigurationManagerInternal()
        {
        }

        string IConfigurationManagerInternal.ApplicationConfigUri
        {
            get
            {
                return ClientConfigPaths.Current.ApplicationConfigUri;
            }
        }

        string IConfigurationManagerInternal.ExeLocalConfigDirectory
        {
            get
            {
                return ClientConfigPaths.Current.LocalConfigDirectory;
            }
        }

        string IConfigurationManagerInternal.ExeLocalConfigPath
        {
            get
            {
                return ClientConfigPaths.Current.LocalConfigFilename;
            }
        }

        string IConfigurationManagerInternal.ExeProductName
        {
            get
            {
                return ClientConfigPaths.Current.ProductName;
            }
        }

        string IConfigurationManagerInternal.ExeProductVersion
        {
            get
            {
                return ClientConfigPaths.Current.ProductVersion;
            }
        }

        string IConfigurationManagerInternal.ExeRoamingConfigDirectory
        {
            get
            {
                return ClientConfigPaths.Current.RoamingConfigDirectory;
            }
        }

        string IConfigurationManagerInternal.ExeRoamingConfigPath
        {
            get
            {
                return ClientConfigPaths.Current.RoamingConfigFilename;
            }
        }

        string IConfigurationManagerInternal.MachineConfigPath
        {
            get
            {
                return ClientConfigurationHost.MachineConfigFilePath;
            }
        }

        bool IConfigurationManagerInternal.SetConfigurationSystemInProgress
        {
            get
            {
                return ConfigurationManager.SetConfigurationSystemInProgress;
            }
        }

        bool IConfigurationManagerInternal.SupportsUserConfig
        {
            get
            {
                return ConfigurationManager.SupportsUserConfig;
            }
        }

        string IConfigurationManagerInternal.UserConfigFilename
        {
            get
            {
                return "user.config";
            }
        }
    }
}

