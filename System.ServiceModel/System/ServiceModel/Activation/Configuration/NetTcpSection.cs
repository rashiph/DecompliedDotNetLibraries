namespace System.ServiceModel.Activation.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Configuration;

    public sealed class NetTcpSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static NetTcpSection GetSection()
        {
            NetTcpSection section = (NetTcpSection) ConfigurationManager.GetSection(System.ServiceModel.Activation.Configuration.ConfigurationStrings.NetTcpSectionPath);
            if (section == null)
            {
                section = new NetTcpSection();
            }
            return section;
        }

        protected override void InitializeDefault()
        {
            this.AllowAccounts.SetDefaultIdentifiers();
        }

        [ConfigurationProperty("allowAccounts")]
        public SecurityIdentifierElementCollection AllowAccounts
        {
            get
            {
                return (SecurityIdentifierElementCollection) base["allowAccounts"];
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("listenBacklog", DefaultValue=10)]
        public int ListenBacklog
        {
            get
            {
                return (int) base["listenBacklog"];
            }
            set
            {
                base["listenBacklog"] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxPendingAccepts", DefaultValue=2)]
        public int MaxPendingAccepts
        {
            get
            {
                return (int) base["maxPendingAccepts"];
            }
            set
            {
                base["maxPendingAccepts"] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxPendingConnections", DefaultValue=100)]
        public int MaxPendingConnections
        {
            get
            {
                return (int) base["maxPendingConnections"];
            }
            set
            {
                base["maxPendingConnections"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("allowAccounts", typeof(SecurityIdentifierElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("listenBacklog", typeof(int), 10, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxPendingConnections", typeof(int), 100, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxPendingAccepts", typeof(int), 2, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("receiveTimeout", typeof(TimeSpan), TimeSpan.Parse("00:00:10", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("teredoEnabled", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("receiveTimeout", DefaultValue="00:00:10"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan ReceiveTimeout
        {
            get
            {
                return (TimeSpan) base["receiveTimeout"];
            }
            set
            {
                base["receiveTimeout"] = value;
            }
        }

        [ConfigurationProperty("teredoEnabled", DefaultValue=false)]
        public bool TeredoEnabled
        {
            get
            {
                return (bool) base["teredoEnabled"];
            }
            set
            {
                base["teredoEnabled"] = value;
            }
        }
    }
}

