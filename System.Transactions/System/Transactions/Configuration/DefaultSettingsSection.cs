namespace System.Transactions.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Transactions;

    public sealed class DefaultSettingsSection : ConfigurationSection
    {
        internal static DefaultSettingsSection GetSection()
        {
            DefaultSettingsSection section = (DefaultSettingsSection) System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.DefaultSettingsSectionPath);
            if (section == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, System.Transactions.SR.GetString("ConfigurationSectionNotFound"), new object[] { ConfigurationStrings.DefaultSettingsSectionPath }));
            }
            return section;
        }

        [ConfigurationProperty("distributedTransactionManagerName", DefaultValue="")]
        public string DistributedTransactionManagerName
        {
            get
            {
                return (string) base["distributedTransactionManagerName"];
            }
            set
            {
                base["distributedTransactionManagerName"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                propertys.Add(new ConfigurationProperty("distributedTransactionManagerName", typeof(string), "", ConfigurationPropertyOptions.None));
                propertys.Add(new ConfigurationProperty("timeout", typeof(TimeSpan), "00:01:00", null, new TimeSpanValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                return propertys;
            }
        }

        [ConfigurationProperty("timeout", DefaultValue="00:01:00"), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807")]
        public TimeSpan Timeout
        {
            get
            {
                return (TimeSpan) base["timeout"];
            }
            set
            {
                if (!ConfigurationStrings.IsValidTimeSpan(value))
                {
                    throw new ArgumentOutOfRangeException("Timeout", System.Transactions.SR.GetString("ConfigInvalidTimeSpanValue"));
                }
                base["timeout"] = value;
            }
        }
    }
}

