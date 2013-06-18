namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class ReliableSessionElement : BindingElementExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            ReliableSessionBindingElement element = (ReliableSessionBindingElement) bindingElement;
            element.AcknowledgementInterval = this.AcknowledgementInterval;
            element.FlowControlEnabled = this.FlowControlEnabled;
            element.InactivityTimeout = this.InactivityTimeout;
            element.MaxPendingChannels = this.MaxPendingChannels;
            element.MaxRetryCount = this.MaxRetryCount;
            element.MaxTransferWindowSize = this.MaxTransferWindowSize;
            element.Ordered = this.Ordered;
            element.ReliableMessagingVersion = this.ReliableMessagingVersion;
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            ReliableSessionElement element = (ReliableSessionElement) from;
            this.AcknowledgementInterval = element.AcknowledgementInterval;
            this.FlowControlEnabled = element.FlowControlEnabled;
            this.InactivityTimeout = element.InactivityTimeout;
            this.MaxPendingChannels = element.MaxPendingChannels;
            this.MaxRetryCount = element.MaxRetryCount;
            this.MaxTransferWindowSize = element.MaxTransferWindowSize;
            this.Ordered = element.Ordered;
            this.ReliableMessagingVersion = element.ReliableMessagingVersion;
        }

        protected internal override BindingElement CreateBindingElement()
        {
            ReliableSessionBindingElement bindingElement = new ReliableSessionBindingElement();
            this.ApplyConfiguration(bindingElement);
            return bindingElement;
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            ReliableSessionBindingElement element = (ReliableSessionBindingElement) bindingElement;
            this.AcknowledgementInterval = element.AcknowledgementInterval;
            this.FlowControlEnabled = element.FlowControlEnabled;
            this.InactivityTimeout = element.InactivityTimeout;
            this.MaxPendingChannels = element.MaxPendingChannels;
            this.MaxRetryCount = element.MaxRetryCount;
            this.MaxTransferWindowSize = element.MaxTransferWindowSize;
            this.Ordered = element.Ordered;
            this.ReliableMessagingVersion = element.ReliableMessagingVersion;
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("acknowledgementInterval", DefaultValue="00:00:00.2"), ServiceModelTimeSpanValidator(MinValueString="00:00:00.0000001")]
        public TimeSpan AcknowledgementInterval
        {
            get
            {
                return (TimeSpan) base["acknowledgementInterval"];
            }
            set
            {
                base["acknowledgementInterval"] = value;
            }
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(ReliableSessionBindingElement);
            }
        }

        [ConfigurationProperty("flowControlEnabled", DefaultValue=true)]
        public bool FlowControlEnabled
        {
            get
            {
                return (bool) base["flowControlEnabled"];
            }
            set
            {
                base["flowControlEnabled"] = value;
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00.0000001"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("inactivityTimeout", DefaultValue="00:10:00")]
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

        [IntegerValidator(MinValue=1, MaxValue=0x4000), ConfigurationProperty("maxPendingChannels", DefaultValue=4)]
        public int MaxPendingChannels
        {
            get
            {
                return (int) base["maxPendingChannels"];
            }
            set
            {
                base["maxPendingChannels"] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("maxRetryCount", DefaultValue=8)]
        public int MaxRetryCount
        {
            get
            {
                return (int) base["maxRetryCount"];
            }
            set
            {
                base["maxRetryCount"] = value;
            }
        }

        [ConfigurationProperty("maxTransferWindowSize", DefaultValue=8), IntegerValidator(MinValue=1, MaxValue=0x1000)]
        public int MaxTransferWindowSize
        {
            get
            {
                return (int) base["maxTransferWindowSize"];
            }
            set
            {
                base["maxTransferWindowSize"] = value;
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
                    propertys.Add(new ConfigurationProperty("acknowledgementInterval", typeof(TimeSpan), TimeSpan.Parse("00:00:00.2", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("flowControlEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("inactivityTimeout", typeof(TimeSpan), TimeSpan.Parse("00:10:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00.0000001", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxPendingChannels", typeof(int), 4, null, new IntegerValidator(1, 0x4000, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxRetryCount", typeof(int), 8, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxTransferWindowSize", typeof(int), 8, null, new IntegerValidator(1, 0x1000, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("ordered", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("reliableMessagingVersion", typeof(System.ServiceModel.ReliableMessagingVersion), "WSReliableMessagingFebruary2005", new ReliableMessagingVersionConverter(), null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("reliableMessagingVersion", DefaultValue="WSReliableMessagingFebruary2005"), TypeConverter(typeof(ReliableMessagingVersionConverter))]
        public System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return (System.ServiceModel.ReliableMessagingVersion) base["reliableMessagingVersion"];
            }
            set
            {
                base["reliableMessagingVersion"] = value;
            }
        }
    }
}

