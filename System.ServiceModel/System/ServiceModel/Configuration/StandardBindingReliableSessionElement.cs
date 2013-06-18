namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;

    public class StandardBindingReliableSessionElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public void ApplyConfiguration(ReliableSession reliableSession)
        {
            if (reliableSession == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSession");
            }
            reliableSession.Ordered = this.Ordered;
            reliableSession.InactivityTimeout = this.InactivityTimeout;
        }

        public void InitializeFrom(ReliableSession reliableSession)
        {
            if (reliableSession == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reliableSession");
            }
            this.Ordered = reliableSession.Ordered;
            this.InactivityTimeout = reliableSession.InactivityTimeout;
        }

        [ConfigurationProperty("inactivityTimeout", DefaultValue="00:10:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00.0000001"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan InactivityTimeout
        {
            get
            {
                return (TimeSpan) base["inactivityTimeout"];
            }
            set
            {
                base["inactivityTimeout"] = value;
            }
        }

        [ConfigurationProperty("ordered", DefaultValue=true)]
        public bool Ordered
        {
            get
            {
                return (bool) base["ordered"];
            }
            set
            {
                base["ordered"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("ordered", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("inactivityTimeout", typeof(TimeSpan), TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

