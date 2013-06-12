namespace System.Data
{
    using System;
    using System.Configuration;

    internal sealed class LocalDBInstanceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired=true)]
        public string Name
        {
            get
            {
                return (base["name"] as string);
            }
        }

        [ConfigurationProperty("version", IsRequired=true)]
        public string Version
        {
            get
            {
                return (base["version"] as string);
            }
        }
    }
}

