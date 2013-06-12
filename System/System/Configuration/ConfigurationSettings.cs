namespace System.Configuration
{
    using System;
    using System.Collections.Specialized;

    public sealed class ConfigurationSettings
    {
        private ConfigurationSettings()
        {
        }

        [Obsolete("This method is obsolete, it has been replaced by System.Configuration!System.Configuration.ConfigurationManager.GetSection")]
        public static object GetConfig(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

        [Obsolete("This method is obsolete, it has been replaced by System.Configuration!System.Configuration.ConfigurationManager.AppSettings")]
        public static NameValueCollection AppSettings
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }
    }
}

