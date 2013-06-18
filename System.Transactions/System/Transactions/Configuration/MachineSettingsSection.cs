namespace System.Transactions.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Transactions;

    public sealed class MachineSettingsSection : ConfigurationSection
    {
        internal static MachineSettingsSection GetSection()
        {
            MachineSettingsSection section = (MachineSettingsSection) System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.MachineSettingsSectionPath);
            if (section == null)
            {
                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, System.Transactions.SR.GetString("ConfigurationSectionNotFound"), new object[] { ConfigurationStrings.MachineSettingsSectionPath }));
            }
            return section;
        }

        [ConfigurationProperty("maxTimeout", DefaultValue="00:10:00"), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807")]
        public TimeSpan MaxTimeout
        {
            get
            {
                return (TimeSpan) base["maxTimeout"];
            }
            set
            {
                if (!ConfigurationStrings.IsValidTimeSpan(value))
                {
                    throw new ArgumentOutOfRangeException("MaxTimeout", System.Transactions.SR.GetString("ConfigInvalidTimeSpanValue"));
                }
                base["maxTimeout"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                propertys.Add(new ConfigurationProperty("maxTimeout", typeof(TimeSpan), "00:10:00", null, new TimeSpanValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                return propertys;
            }
        }
    }
}

