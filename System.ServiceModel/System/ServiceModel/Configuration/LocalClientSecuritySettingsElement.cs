namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public sealed class LocalClientSecuritySettingsElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(LocalClientSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            settings.CacheCookies = this.CacheCookies;
            if (base.ElementInformation.Properties["detectReplays"].ValueOrigin != PropertyValueOrigin.Default)
            {
                settings.DetectReplays = this.DetectReplays;
            }
            settings.MaxClockSkew = this.MaxClockSkew;
            settings.MaxCookieCachingTime = this.MaxCookieCachingTime;
            settings.ReconnectTransportOnFailure = this.ReconnectTransportOnFailure;
            settings.ReplayCacheSize = this.ReplayCacheSize;
            settings.ReplayWindow = this.ReplayWindow;
            settings.SessionKeyRenewalInterval = this.SessionKeyRenewalInterval;
            settings.SessionKeyRolloverInterval = this.SessionKeyRolloverInterval;
            settings.TimestampValidityDuration = this.TimestampValidityDuration;
            settings.CookieRenewalThresholdPercentage = this.CookieRenewalThresholdPercentage;
        }

        internal void CopyFrom(LocalClientSecuritySettingsElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.CacheCookies = source.CacheCookies;
            if (source.ElementInformation.Properties["detectReplays"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.DetectReplays = source.DetectReplays;
            }
            this.MaxClockSkew = source.MaxClockSkew;
            this.MaxCookieCachingTime = source.MaxCookieCachingTime;
            this.ReconnectTransportOnFailure = source.ReconnectTransportOnFailure;
            this.ReplayCacheSize = source.ReplayCacheSize;
            this.ReplayWindow = source.ReplayWindow;
            this.SessionKeyRenewalInterval = source.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = source.SessionKeyRolloverInterval;
            this.TimestampValidityDuration = source.TimestampValidityDuration;
            this.CookieRenewalThresholdPercentage = source.CookieRenewalThresholdPercentage;
        }

        internal void InitializeFrom(LocalClientSecuritySettings settings)
        {
            if (settings == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settings");
            }
            this.CacheCookies = settings.CacheCookies;
            this.DetectReplays = settings.DetectReplays;
            this.MaxClockSkew = settings.MaxClockSkew;
            this.MaxCookieCachingTime = settings.MaxCookieCachingTime;
            this.ReconnectTransportOnFailure = settings.ReconnectTransportOnFailure;
            this.ReplayCacheSize = settings.ReplayCacheSize;
            this.ReplayWindow = settings.ReplayWindow;
            this.SessionKeyRenewalInterval = settings.SessionKeyRenewalInterval;
            this.SessionKeyRolloverInterval = settings.SessionKeyRolloverInterval;
            this.TimestampValidityDuration = settings.TimestampValidityDuration;
            this.CookieRenewalThresholdPercentage = settings.CookieRenewalThresholdPercentage;
        }

        [ConfigurationProperty("cacheCookies", DefaultValue=true)]
        public bool CacheCookies
        {
            get
            {
                return (bool) base["cacheCookies"];
            }
            set
            {
                base["cacheCookies"] = value;
            }
        }

        [IntegerValidator(MinValue=0, MaxValue=100), ConfigurationProperty("cookieRenewalThresholdPercentage", DefaultValue=60)]
        public int CookieRenewalThresholdPercentage
        {
            get
            {
                return (int) base["cookieRenewalThresholdPercentage"];
            }
            set
            {
                base["cookieRenewalThresholdPercentage"] = value;
            }
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

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("maxClockSkew", DefaultValue="00:05:00")]
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

        [ConfigurationProperty("maxCookieCachingTime", DefaultValue="10675199.02:48:05.4775807"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan MaxCookieCachingTime
        {
            get
            {
                return (TimeSpan) base["maxCookieCachingTime"];
            }
            set
            {
                base["maxCookieCachingTime"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("cacheCookies", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("detectReplays", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("replayCacheSize", typeof(int), 0xdbba0, null, new IntegerValidator(1, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxClockSkew", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("maxCookieCachingTime", typeof(TimeSpan), TimeSpan.Parse("10675199.02:48:05.4775807", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("replayWindow", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("sessionKeyRenewalInterval", typeof(TimeSpan), TimeSpan.Parse("10:00:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("sessionKeyRolloverInterval", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("reconnectTransportOnFailure", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("timestampValidityDuration", typeof(TimeSpan), TimeSpan.Parse("00:05:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Parse("00:00:00", CultureInfo.InvariantCulture), TimeSpan.Parse("24.20:31:23.6470000", CultureInfo.InvariantCulture)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("cookieRenewalThresholdPercentage", typeof(int), 60, null, new IntegerValidator(0, 100, false), ConfigurationPropertyOptions.None));
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

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("sessionKeyRenewalInterval", DefaultValue="10:00:00")]
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

        [ConfigurationProperty("timestampValidityDuration", DefaultValue="00:05:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
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

