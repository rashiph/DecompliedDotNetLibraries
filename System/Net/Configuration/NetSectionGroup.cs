namespace System.Net.Configuration
{
    using System;
    using System.Configuration;

    public sealed class NetSectionGroup : ConfigurationSectionGroup
    {
        public static NetSectionGroup GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return (config.GetSectionGroup("system.net") as NetSectionGroup);
        }

        [ConfigurationProperty("authenticationModules")]
        public AuthenticationModulesSection AuthenticationModules
        {
            get
            {
                return (AuthenticationModulesSection) base.Sections["authenticationModules"];
            }
        }

        [ConfigurationProperty("connectionManagement")]
        public ConnectionManagementSection ConnectionManagement
        {
            get
            {
                return (ConnectionManagementSection) base.Sections["connectionManagement"];
            }
        }

        [ConfigurationProperty("defaultProxy")]
        public DefaultProxySection DefaultProxy
        {
            get
            {
                return (DefaultProxySection) base.Sections["defaultProxy"];
            }
        }

        public MailSettingsSectionGroup MailSettings
        {
            get
            {
                return (MailSettingsSectionGroup) base.SectionGroups["mailSettings"];
            }
        }

        [ConfigurationProperty("requestCaching")]
        public RequestCachingSection RequestCaching
        {
            get
            {
                return (RequestCachingSection) base.Sections["requestCaching"];
            }
        }

        [ConfigurationProperty("settings")]
        public SettingsSection Settings
        {
            get
            {
                return (SettingsSection) base.Sections["settings"];
            }
        }

        [ConfigurationProperty("webRequestModules")]
        public WebRequestModulesSection WebRequestModules
        {
            get
            {
                return (WebRequestModulesSection) base.Sections["webRequestModules"];
            }
        }
    }
}

