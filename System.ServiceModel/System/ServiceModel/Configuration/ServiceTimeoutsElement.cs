namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Description;

    public sealed class ServiceTimeoutsElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ServiceTimeoutsElement element = (ServiceTimeoutsElement) from;
            this.TransactionTimeout = element.TransactionTimeout;
        }

        protected internal override object CreateBehavior()
        {
            return new ServiceTimeoutsBehavior(this.TransactionTimeout);
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(ServiceTimeoutsBehavior);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("transactionTimeout", typeof(TimeSpan), TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("transactionTimeout", DefaultValue="00:00:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan TransactionTimeout
        {
            get
            {
                return (TimeSpan) base["transactionTimeout"];
            }
            set
            {
                base["transactionTimeout"] = value;
            }
        }
    }
}

