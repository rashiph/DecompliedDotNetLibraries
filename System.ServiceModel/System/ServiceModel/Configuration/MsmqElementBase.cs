namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract class MsmqElementBase : TransportElement
    {
        private ConfigurationPropertyCollection properties;

        protected MsmqElementBase()
        {
        }

        public override void ApplyConfiguration(BindingElement bindingElement)
        {
            base.ApplyConfiguration(bindingElement);
            System.ServiceModel.Channels.MsmqBindingElementBase base2 = bindingElement as System.ServiceModel.Channels.MsmqBindingElementBase;
            if (base2 != null)
            {
                if (null != this.CustomDeadLetterQueue)
                {
                    base2.CustomDeadLetterQueue = this.CustomDeadLetterQueue;
                }
                base2.DeadLetterQueue = this.DeadLetterQueue;
                base2.Durable = this.Durable;
                base2.ExactlyOnce = this.ExactlyOnce;
                base2.MaxRetryCycles = this.MaxRetryCycles;
                base2.ReceiveContextEnabled = this.ReceiveContextEnabled;
                base2.ReceiveErrorHandling = this.ReceiveErrorHandling;
                base2.ReceiveRetryCount = this.ReceiveRetryCount;
                base2.RetryCycleDelay = this.RetryCycleDelay;
                base2.TimeToLive = this.TimeToLive;
                base2.UseSourceJournal = this.UseSourceJournal;
                base2.UseMsmqTracing = this.UseMsmqTracing;
                base2.ValidityDuration = this.ValidityDuration;
                this.MsmqTransportSecurity.ApplyConfiguration(base2.MsmqTransportSecurity);
            }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);
            MsmqElementBase base2 = from as MsmqElementBase;
            if (base2 != null)
            {
                this.CustomDeadLetterQueue = base2.CustomDeadLetterQueue;
                this.DeadLetterQueue = base2.DeadLetterQueue;
                this.Durable = base2.Durable;
                this.ExactlyOnce = base2.ExactlyOnce;
                this.MaxRetryCycles = base2.MaxRetryCycles;
                this.ReceiveContextEnabled = base2.ReceiveContextEnabled;
                this.ReceiveErrorHandling = base2.ReceiveErrorHandling;
                this.ReceiveRetryCount = base2.ReceiveRetryCount;
                this.RetryCycleDelay = base2.RetryCycleDelay;
                this.TimeToLive = base2.TimeToLive;
                this.UseSourceJournal = base2.UseSourceJournal;
                this.UseMsmqTracing = base2.UseMsmqTracing;
                this.ValidityDuration = base2.ValidityDuration;
                this.MsmqTransportSecurity.MsmqAuthenticationMode = base2.MsmqTransportSecurity.MsmqAuthenticationMode;
                this.MsmqTransportSecurity.MsmqProtectionLevel = base2.MsmqTransportSecurity.MsmqProtectionLevel;
                this.MsmqTransportSecurity.MsmqEncryptionAlgorithm = base2.MsmqTransportSecurity.MsmqEncryptionAlgorithm;
                this.MsmqTransportSecurity.MsmqSecureHashAlgorithm = base2.MsmqTransportSecurity.MsmqSecureHashAlgorithm;
            }
        }

        protected internal override void InitializeFrom(BindingElement bindingElement)
        {
            base.InitializeFrom(bindingElement);
            System.ServiceModel.Channels.MsmqBindingElementBase base2 = bindingElement as System.ServiceModel.Channels.MsmqBindingElementBase;
            if (base2 != null)
            {
                if (null != base2.CustomDeadLetterQueue)
                {
                    this.CustomDeadLetterQueue = base2.CustomDeadLetterQueue;
                }
                this.DeadLetterQueue = base2.DeadLetterQueue;
                this.Durable = base2.Durable;
                this.ExactlyOnce = base2.ExactlyOnce;
                this.MaxRetryCycles = base2.MaxRetryCycles;
                this.ReceiveErrorHandling = base2.ReceiveErrorHandling;
                this.ReceiveRetryCount = base2.ReceiveRetryCount;
                this.RetryCycleDelay = base2.RetryCycleDelay;
                this.TimeToLive = base2.TimeToLive;
                this.UseSourceJournal = base2.UseSourceJournal;
                if (!base2.ReceiveContextEnabled)
                {
                    this.ReceiveContextEnabled = base2.ReceiveContextEnabled;
                }
                this.UseMsmqTracing = base2.UseMsmqTracing;
                if (base2.ValidityDuration != MsmqDefaults.ValidityDuration)
                {
                    this.ValidityDuration = base2.ValidityDuration;
                }
                this.MsmqTransportSecurity.InitializeFrom(base2.MsmqTransportSecurity);
            }
        }

        [ConfigurationProperty("customDeadLetterQueue", DefaultValue=null)]
        public Uri CustomDeadLetterQueue
        {
            get
            {
                return (Uri) base["customDeadLetterQueue"];
            }
            set
            {
                base["customDeadLetterQueue"] = value;
            }
        }

        [ConfigurationProperty("deadLetterQueue", DefaultValue=1), ServiceModelEnumValidator(typeof(DeadLetterQueueHelper))]
        public System.ServiceModel.DeadLetterQueue DeadLetterQueue
        {
            get
            {
                return (System.ServiceModel.DeadLetterQueue) base["deadLetterQueue"];
            }
            set
            {
                base["deadLetterQueue"] = value;
            }
        }

        [ConfigurationProperty("durable", DefaultValue=true)]
        public bool Durable
        {
            get
            {
                return (bool) base["durable"];
            }
            set
            {
                base["durable"] = value;
            }
        }

        [ConfigurationProperty("exactlyOnce", DefaultValue=true)]
        public bool ExactlyOnce
        {
            get
            {
                return (bool) base["exactlyOnce"];
            }
            set
            {
                base["exactlyOnce"] = value;
            }
        }

        [ConfigurationProperty("maxRetryCycles", DefaultValue=2), IntegerValidator(MinValue=0)]
        public int MaxRetryCycles
        {
            get
            {
                return (int) base["maxRetryCycles"];
            }
            set
            {
                base["maxRetryCycles"] = value;
            }
        }

        [ConfigurationProperty("msmqTransportSecurity")]
        public MsmqTransportSecurityElement MsmqTransportSecurity
        {
            get
            {
                return (MsmqTransportSecurityElement) base["msmqTransportSecurity"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("customDeadLetterQueue", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("deadLetterQueue", typeof(System.ServiceModel.DeadLetterQueue), System.ServiceModel.DeadLetterQueue.System, null, new ServiceModelEnumValidator(typeof(DeadLetterQueueHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("durable", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("exactlyOnce", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("maxRetryCycles", typeof(int), 2, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveContextEnabled", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveErrorHandling", typeof(System.ServiceModel.ReceiveErrorHandling), System.ServiceModel.ReceiveErrorHandling.Fault, null, new ServiceModelEnumValidator(typeof(ReceiveErrorHandlingHelper)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("receiveRetryCount", typeof(int), 5, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("retryCycleDelay", typeof(TimeSpan), TimeSpan.Parse("00:30:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("msmqTransportSecurity", typeof(MsmqTransportSecurityElement), null, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("timeToLive", typeof(TimeSpan), TimeSpan.Parse("1.00:00:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useSourceJournal", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("useMsmqTracing", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("validityDuration", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("receiveContextEnabled", DefaultValue=true)]
        public bool ReceiveContextEnabled
        {
            get
            {
                return (bool) base["receiveContextEnabled"];
            }
            set
            {
                base["receiveContextEnabled"] = value;
            }
        }

        [ConfigurationProperty("receiveErrorHandling", DefaultValue=0), ServiceModelEnumValidator(typeof(ReceiveErrorHandlingHelper))]
        public System.ServiceModel.ReceiveErrorHandling ReceiveErrorHandling
        {
            get
            {
                return (System.ServiceModel.ReceiveErrorHandling) base["receiveErrorHandling"];
            }
            set
            {
                base["receiveErrorHandling"] = value;
            }
        }

        [ConfigurationProperty("receiveRetryCount", DefaultValue=5), IntegerValidator(MinValue=0)]
        public int ReceiveRetryCount
        {
            get
            {
                return (int) base["receiveRetryCount"];
            }
            set
            {
                base["receiveRetryCount"] = value;
            }
        }

        [ConfigurationProperty("retryCycleDelay", DefaultValue="00:30:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan RetryCycleDelay
        {
            get
            {
                return (TimeSpan) base["retryCycleDelay"];
            }
            set
            {
                base["retryCycleDelay"] = value;
            }
        }

        [ConfigurationProperty("timeToLive", DefaultValue="1.00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan TimeToLive
        {
            get
            {
                return (TimeSpan) base["timeToLive"];
            }
            set
            {
                base["timeToLive"] = value;
            }
        }

        [ConfigurationProperty("useMsmqTracing", DefaultValue=false)]
        public bool UseMsmqTracing
        {
            get
            {
                return (bool) base["useMsmqTracing"];
            }
            set
            {
                base["useMsmqTracing"] = value;
            }
        }

        [ConfigurationProperty("useSourceJournal", DefaultValue=false)]
        public bool UseSourceJournal
        {
            get
            {
                return (bool) base["useSourceJournal"];
            }
            set
            {
                base["useSourceJournal"] = value;
            }
        }

        [ConfigurationProperty("validityDuration", DefaultValue="00:05:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan ValidityDuration
        {
            get
            {
                return (TimeSpan) base["validityDuration"];
            }
            set
            {
                base["validityDuration"] = value;
            }
        }
    }
}

