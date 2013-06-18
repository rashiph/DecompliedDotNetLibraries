namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    internal class MachineSettingsSection : ConfigurationSection
    {
        private static bool enableLoggingKnownPii;
        private const string enableLoggingKnownPiiKey = "enableLoggingKnownPii";
        private static bool hasInitialized = false;
        private ConfigurationPropertyCollection properties;
        private static object syncRoot = new object();

        public static bool EnableLoggingKnownPii
        {
            get
            {
                if (!hasInitialized)
                {
                    lock (syncRoot)
                    {
                        if (!hasInitialized)
                        {
                            MachineSettingsSection section = (MachineSettingsSection) ConfigurationManager.GetSection("system.serviceModel/machineSettings");
                            enableLoggingKnownPii = (bool) section["enableLoggingKnownPii"];
                            hasInitialized = true;
                        }
                    }
                }
                return enableLoggingKnownPii;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("enableLoggingKnownPii", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

