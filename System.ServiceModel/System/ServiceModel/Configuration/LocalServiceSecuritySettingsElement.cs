namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class LocalServiceSecuritySettingsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(LocalServiceSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            if (base.ElementInformation.Properties["detectReplays"].ValueOrigin != PropertyValueOrigin.Default)
            {
                settings.DetectReplays = this.DetectReplays;
            }
            settings.IssuedCookieLifetime = this.IssuedCookieLifetime;
            settings.MaxClockSkew = this.MaxClockSkew;
            settings.MaxPendingSessions = this.MaxPendingSessions;
            settings.MaxStatefulNegotiations = this.MaxStatefulNegotiations;
            settings.NegotiationTimeout = this.NegotiationTimeout;
            settings.ReconnectTransportOnFailure = this.ReconnectTransportOnFailure;
            settings.ReplayCacheSize = this.ReplayCacheSize;
            settings.ReplayWindow = this.ReplayWindow;
            settings.SessionKeyRenewalInterval = this.SessionKeyRenewalInterval;
            settings.SessionKeyRolloverInterval = this.SessionKeyRolloverInterval;
            settings.InactivityTimeout = this.InactivityTimeout;
            settings.TimestampValidityDuration = this.TimestampValidityDuration;
            settings.MaxCachedCookies = this.MaxCachedCookies;
        }

        internal void CopyFrom(LocalServiceSecuritySettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            if (source.ElementInformation.Properties["detectReplays"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.DetectReplays = source.DetectReplays;
            }
            this.IssuedCookieLifetime = source.IssuedCookieLifetime;
            this.MaxClockSkew = source.MaxClockSkew;
            this.MaxPendingSessions = source.MaxPendingSessions;
            this.MaxStatefulNegotiations = source.MaxStatefulNegotiations;
            this.NegotiationTimeout = source.NegotiationTimeout;
            this.ReconnectTransportOnFailure = source.ReconnectTransportOnFailure;
            this.ReplayCacheSize = source.ReplayCacheSize;
            this.ReplayWindow = source.ReplayWindow;
            this.SessionKeyRenewalInterval = source.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = source.SessionKeyRolloverInterval;
            this.InactivityTimeout = source.InactivityTimeout;
            this.TimestampValidityDuration = source.TimestampValidityDuration;
            this.MaxCachedCookies = source.MaxCachedCookies;
        }

        internal void InitializeFrom(LocalServiceSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            this.DetectReplays = settings.DetectReplays;
            this.IssuedCookieLifetime = settings.IssuedCookieLifetime;
            this.MaxClockSkew = settings.MaxClockSkew;
            this.MaxPendingSessions = settings.MaxPendingSessions;
            this.MaxStatefulNegotiations = settings.MaxStatefulNegotiations;
            this.NegotiationTimeout = settings.NegotiationTimeout;
            this.ReconnectTransportOnFailure = settings.ReconnectTransportOnFailure;
            this.ReplayCacheSize = settings.ReplayCacheSize;
            this.ReplayWindow = settings.ReplayWindow;
            this.SessionKeyRenewalInterval = settings.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = settings.SessionKeyRolloverInterval;
            this.InactivityTimeout = settings.InactivityTimeout;
            this.TimestampValidityDuration = settings.TimestampValidityDuration;
            this.MaxCachedCookies = settings.MaxCachedCookies;
        }

        [ConfigurationProperty("detectReplays", DefaultValue=true)]
        public bool DetectReplays
        {
            get
            {
                return (bool) base["detectReplays"];
            }
            set
            {
                base["detectReplays"] = value;
            }
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("inactivityTimeout", DefaultValue="00:02:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
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

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("issuedCookieLifetime", DefaultValue="10:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan IssuedCookieLifetime
        {
            get
            {
                return (TimeSpan) base["issuedCookieLifetime"];
            }
            set
            {
                base["issuedCookieLifetime"] = value;
            }
        }

        [IntegerValidator(MinValue=0), ConfigurationProperty("maxCachedCookies", DefaultValue=0x3e8)]
        public int MaxCachedCookies
        {
            get
            {
                return (int) base["maxCachedCookies"];
            }
            set
            {
                base["maxCachedCookies"] = value;
            }
        }

        [ConfigurationProperty("maxClockSkew", DefaultValue="00:05:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan MaxClockSkew
        {
            get
            {
                return (TimeSpan) base["maxClockSkew"];
            }
            set
            {
                base["maxClockSkew"] = value;
            }
        }

        [ConfigurationProperty("maxPendingSessions", DefaultValue=0x80), IntegerValidator(MinValue=1)]
        public int MaxPendingSessions
        {
            get
            {
                return (int) base["maxPendingSessions"];
            }
            set
            {
                base["maxPendingSessions"] = value;
            }
        }

        [ConfigurationProperty("maxStatefulNegotiations", DefaultValue=0x80), IntegerValidator(MinValue=0)]
        public int MaxStatefulNegotiations
        {
            get
            {
                return (int) base["maxStatefulNegotiations"];
            }
            set
            {
                base["maxStatefulNegotiations"] = value;
            }
        }

        [ConfigurationProperty("negotiationTimeout", DefaultValue="00:01:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan NegotiationTimeout
        {
            get
            {
                return (TimeSpan) base["negotiationTimeout"];
            }
            set
            {
                base["negotiationTimeout"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("detectReplays", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuedCookieLifetime", typeof(TimeSpan), TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxStatefulNegotiations", typeof(int), 0x80, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("replayCacheSize", typeof(int), 0xdbba0, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxClockSkew", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("negotiationTimeout", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("replayWindow", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("inactivityTimeout", typeof(TimeSpan), TimeSpan.Parse("00:02:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("sessionKeyRenewalInterval", typeof(TimeSpan), TimeSpan.Parse("15:00:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("sessionKeyRolloverInterval", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("reconnectTransportOnFailure", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxPendingSessions", typeof(int), 0x80, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxCachedCookies", typeof(int), 0x3e8, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("timestampValidityDuration", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("reconnectTransportOnFailure", DefaultValue=true)]
        public bool ReconnectTransportOnFailure
        {
            get
            {
                return (bool) base["reconnectTransportOnFailure"];
            }
            set
            {
                base["reconnectTransportOnFailure"] = value;
            }
        }

        [IntegerValidator(MinValue=1), ConfigurationProperty("replayCacheSize", DefaultValue=0xdbba0)]
        public int ReplayCacheSize
        {
            get
            {
                return (int) base["replayCacheSize"];
            }
            set
            {
                base["replayCacheSize"] = value;
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("replayWindow", DefaultValue="00:05:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan ReplayWindow
        {
            get
            {
                return (TimeSpan) base["replayWindow"];
            }
            set
            {
                base["replayWindow"] = value;
            }
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("sessionKeyRenewalInterval", DefaultValue="15:00:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan SessionKeyRenewalInterval
        {
            get
            {
                return (TimeSpan) base["sessionKeyRenewalInterval"];
            }
            set
            {
                base["sessionKeyRenewalInterval"] = value;
            }
        }

        [ConfigurationProperty("sessionKeyRolloverInterval", DefaultValue="00:05:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan SessionKeyRolloverInterval
        {
            get
            {
                return (TimeSpan) base["sessionKeyRolloverInterval"];
            }
            set
            {
                base["sessionKeyRolloverInterval"] = value;
            }
        }

        [ConfigurationProperty("timestampValidityDuration", DefaultValue="00:05:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan TimestampValidityDuration
        {
            get
            {
                return (TimeSpan) base["timestampValidityDuration"];
            }
            set
            {
                base["timestampValidityDuration"] = value;
            }
        }
    }
}

