namespace System.Transactions.Configuration
{
    using System;
    using System.Configuration;

    public sealed class TransactionsSectionGroup : ConfigurationSectionGroup
    {
        public static TransactionsSectionGroup GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return (TransactionsSectionGroup) config.GetSectionGroup("system.transactions");
        }

        [ConfigurationProperty("defaultSettings")]
        public DefaultSettingsSection DefaultSettings
        {
            get
            {
                return (DefaultSettingsSection) base.Sections["defaultSettings"];
            }
        }

        [ConfigurationProperty("machineSettings")]
        public MachineSettingsSection MachineSettings
        {
            get
            {
                return (MachineSettingsSection) base.Sections["machineSettings"];
            }
        }
    }
}

