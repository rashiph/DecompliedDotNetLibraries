namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class AuthenticationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propForms = new ConfigurationProperty("forms", typeof(FormsAuthenticationConfiguration), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMode = new ConfigurationProperty("mode", typeof(AuthenticationMode), AuthenticationMode.Windows, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propPassport = new ConfigurationProperty("passport", typeof(PassportAuthentication), null, ConfigurationPropertyOptions.None);
        private AuthenticationMode authenticationModeCache;
        private bool authenticationModeCached;

        static AuthenticationSection()
        {
            _properties.Add(_propForms);
            _properties.Add(_propPassport);
            _properties.Add(_propMode);
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
            this.authenticationModeCached = false;
        }

        internal void ValidateAuthenticationMode()
        {
            if ((this.Mode == AuthenticationMode.Passport) && (UnsafeNativeMethods.PassportVersion() < 0))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Passport_not_installed"));
            }
        }

        [ConfigurationProperty("forms")]
        public FormsAuthenticationConfiguration Forms
        {
            get
            {
                return (FormsAuthenticationConfiguration) base[_propForms];
            }
        }

        [ConfigurationProperty("mode", DefaultValue=1)]
        public AuthenticationMode Mode
        {
            get
            {
                if (!this.authenticationModeCached)
                {
                    this.authenticationModeCache = (AuthenticationMode) base[_propMode];
                    this.authenticationModeCached = true;
                }
                return this.authenticationModeCache;
            }
            set
            {
                base[_propMode] = value;
                this.authenticationModeCache = value;
            }
        }

        [Obsolete("This property is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID."), ConfigurationProperty("passport")]
        public PassportAuthentication Passport
        {
            get
            {
                return (PassportAuthentication) base[_propPassport];
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

