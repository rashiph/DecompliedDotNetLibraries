namespace System.Configuration
{
    using System;
    using System.Security.Permissions;

    [ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
    internal static class PrivilegedConfigurationManager
    {
        internal static object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

        internal static ConnectionStringSettingsCollection ConnectionStrings
        {
            get
            {
                return ConfigurationManager.ConnectionStrings;
            }
        }
    }
}

