namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class ClientTarget : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAlias = new ConfigurationProperty("alias", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propUserAgent = new ConfigurationProperty("userAgent", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);

        static ClientTarget()
        {
            _properties.Add(_propAlias);
            _properties.Add(_propUserAgent);
        }

        internal ClientTarget()
        {
        }

        public ClientTarget(string alias, string userAgent)
        {
            base[_propAlias] = alias;
            base[_propUserAgent] = userAgent;
        }

        [StringValidator(MinLength=1), ConfigurationProperty("alias", IsRequired=true, IsKey=true)]
        public string Alias
        {
            get
            {
                return (string) base[_propAlias];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("userAgent", IsRequired=true), StringValidator(MinLength=1)]
        public string UserAgent
        {
            get
            {
                return (string) base[_propUserAgent];
            }
        }
    }
}

