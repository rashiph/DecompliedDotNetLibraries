namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    internal class ConfigsHelper
    {
        internal static void GetRegistryStringAttribute(ref string val, ConfigurationElement config, string propName)
        {
            if (!System.Web.Configuration.HandlerBase.CheckAndReadRegistryValue(ref val, false))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_registry_config"), config.ElementInformation.Properties[propName].Source, config.ElementInformation.Properties[propName].LineNumber);
            }
        }
    }
}

