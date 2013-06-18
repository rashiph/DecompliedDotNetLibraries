namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Web;
    using System.Web.Security;

    public sealed class AnonymousIdentificationSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propCookieless = new ConfigurationProperty("cookieless", typeof(HttpCookieMode), HttpCookieMode.UseCookies, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieName = new ConfigurationProperty("cookieName", typeof(string), ".ASPXANONYMOUS", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookiePath = new ConfigurationProperty("cookiePath", typeof(string), "/", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieProtection = new ConfigurationProperty("cookieProtection", typeof(System.Web.Security.CookieProtection), System.Web.Security.CookieProtection.Validation, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieRequireSSL = new ConfigurationProperty("cookieRequireSSL", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieSlidingExpiration = new ConfigurationProperty("cookieSlidingExpiration", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCookieTimeout = new ConfigurationProperty("cookieTimeout", typeof(TimeSpan), TimeSpan.FromMinutes(100000.0), StdValidatorsAndConverters.TimeSpanMinutesOrInfiniteConverter, StdValidatorsAndConverters.PositiveTimeSpanValidator, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDomain = new ConfigurationProperty("domain", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnabled = new ConfigurationProperty("enabled", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static AnonymousIdentificationSection()
        {
            _properties.Add(_propEnabled);
            _properties.Add(_propCookieName);
            _properties.Add(_propCookieTimeout);
            _properties.Add(_propCookiePath);
            _properties.Add(_propCookieRequireSSL);
            _properties.Add(_propCookieSlidingExpiration);
            _properties.Add(_propCookieProtection);
            _properties.Add(_propCookieless);
            _properties.Add(_propDomain);
        }

        [ConfigurationProperty("cookieless", DefaultValue=1)]
        public HttpCookieMode Cookieless
        {
            get
            {
                return (HttpCookieMode) base[_propCookieless];
            }
            set
            {
                base[_propCookieless] = value;
            }
        }

        [ConfigurationProperty("cookieName", DefaultValue=".ASPXANONYMOUS"), StringValidator(MinLength=1)]
        public string CookieName
        {
            get
            {
                return (string) base[_propCookieName];
            }
            set
            {
                base[_propCookieName] = value;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("cookiePath", DefaultValue="/")]
        public string CookiePath
        {
            get
            {
                return (string) base[_propCookiePath];
            }
            set
            {
                base[_propCookiePath] = value;
            }
        }

        [ConfigurationProperty("cookieProtection", DefaultValue=1)]
        public System.Web.Security.CookieProtection CookieProtection
        {
            get
            {
                return (System.Web.Security.CookieProtection) base[_propCookieProtection];
            }
            set
            {
                base[_propCookieProtection] = value;
            }
        }

        [ConfigurationProperty("cookieRequireSSL", DefaultValue=false)]
        public bool CookieRequireSSL
        {
            get
            {
                return (bool) base[_propCookieRequireSSL];
            }
            set
            {
                base[_propCookieRequireSSL] = value;
            }
        }

        [ConfigurationProperty("cookieSlidingExpiration", DefaultValue=true)]
        public bool CookieSlidingExpiration
        {
            get
            {
                return (bool) base[_propCookieSlidingExpiration];
            }
            set
            {
                base[_propCookieSlidingExpiration] = value;
            }
        }

        [TypeConverter(typeof(TimeSpanMinutesOrInfiniteConverter)), ConfigurationProperty("cookieTimeout", DefaultValue="69.10:40:00"), TimeSpanValidator(MinValueString="00:00:00", MaxValueString="10675199.02:48:05.4775807")]
        public TimeSpan CookieTimeout
        {
            get
            {
                return (TimeSpan) base[_propCookieTimeout];
            }
            set
            {
                base[_propCookieTimeout] = value;
            }
        }

        [ConfigurationProperty("domain")]
        public string Domain
        {
            get
            {
                return (string) base[_propDomain];
            }
            set
            {
                base[_propDomain] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue=false)]
        public bool Enabled
        {
            get
            {
                return (bool) base[_propEnabled];
            }
            set
            {
                base[_propEnabled] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

