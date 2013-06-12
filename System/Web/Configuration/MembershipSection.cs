namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Web;

    public sealed class MembershipSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propDefaultProvider = new ConfigurationProperty("defaultProvider", typeof(string), "AspNetSqlMembershipProvider", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propHashAlgorithmType = new ConfigurationProperty("hashAlgorithmType", typeof(string), string.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProviders = new ConfigurationProperty("providers", typeof(ProviderSettingsCollection), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propUserIsOnlineTimeWindow = new ConfigurationProperty("userIsOnlineTimeWindow", typeof(TimeSpan), TimeSpan.FromMinutes(15.0), StdValidatorsAndConverters.TimeSpanMinutesConverter, new TimeSpanValidator(TimeSpan.FromMinutes(1.0), TimeSpan.MaxValue), ConfigurationPropertyOptions.None);

        static MembershipSection()
        {
            _properties.Add(_propProviders);
            _properties.Add(_propDefaultProvider);
            _properties.Add(_propUserIsOnlineTimeWindow);
            _properties.Add(_propHashAlgorithmType);
        }

        internal void ThrowHashAlgorithmException()
        {
            throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_hash_algorithm_type", new object[] { this.HashAlgorithmType }), base.ElementInformation.Properties["hashAlgorithmType"].Source, base.ElementInformation.Properties["hashAlgorithmType"].LineNumber);
        }

        [StringValidator(MinLength=1), ConfigurationProperty("defaultProvider", DefaultValue="AspNetSqlMembershipProvider")]
        public string DefaultProvider
        {
            get
            {
                return (string) base[_propDefaultProvider];
            }
            set
            {
                base[_propDefaultProvider] = value;
            }
        }

        [ConfigurationProperty("hashAlgorithmType", DefaultValue="")]
        public string HashAlgorithmType
        {
            get
            {
                return (string) base[_propHashAlgorithmType];
            }
            set
            {
                base[_propHashAlgorithmType] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection) base[_propProviders];
            }
        }

        [TimeSpanValidator(MinValueString="00:01:00", MaxValueString="10675199.02:48:05.4775807"), ConfigurationProperty("userIsOnlineTimeWindow", DefaultValue="00:15:00"), TypeConverter(typeof(TimeSpanMinutesConverter))]
        public TimeSpan UserIsOnlineTimeWindow
        {
            get
            {
                return (TimeSpan) base[_propUserIsOnlineTimeWindow];
            }
            set
            {
                base[_propUserIsOnlineTimeWindow] = value;
            }
        }
    }
}

