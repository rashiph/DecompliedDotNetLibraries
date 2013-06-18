namespace System.Configuration
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public static class ProtectedConfiguration
    {
        public const string DataProtectionProviderName = "DataProtectionConfigurationProvider";
        public const string ProtectedDataSectionName = "configProtectedData";
        public const string RsaProviderName = "RsaProtectedConfigurationProvider";

        public static string DefaultProvider
        {
            get
            {
                ProtectedConfigurationSection section = System.Configuration.PrivilegedConfigurationManager.GetSection("configProtectedData") as ProtectedConfigurationSection;
                if (section != null)
                {
                    return section.DefaultProvider;
                }
                return "";
            }
        }

        public static ProtectedConfigurationProviderCollection Providers
        {
            get
            {
                ProtectedConfigurationSection section = System.Configuration.PrivilegedConfigurationManager.GetSection("configProtectedData") as ProtectedConfigurationSection;
                if (section == null)
                {
                    return new ProtectedConfigurationProviderCollection();
                }
                return section.GetAllProviders();
            }
        }
    }
}

