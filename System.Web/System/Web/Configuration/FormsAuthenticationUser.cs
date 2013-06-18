namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;

    public sealed class FormsAuthenticationUser : ConfigurationElement
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propName = new ConfigurationProperty("name", typeof(string), "", new LowerCaseStringConverter(), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propPassword = new ConfigurationProperty("password", typeof(string), "", ConfigurationPropertyOptions.IsRequired);

        static FormsAuthenticationUser()
        {
            _properties.Add(_propName);
            _properties.Add(_propPassword);
        }

        internal FormsAuthenticationUser()
        {
        }

        public FormsAuthenticationUser(string name, string password) : this()
        {
            this.Name = name.ToLower(CultureInfo.InvariantCulture);
            this.Password = password;
        }

        [StringValidator, ConfigurationProperty("name", IsRequired=true, IsKey=true, DefaultValue=""), TypeConverter(typeof(LowerCaseStringConverter))]
        public string Name
        {
            get
            {
                return (string) base[_propName];
            }
            set
            {
                base[_propName] = value;
            }
        }

        [ConfigurationProperty("password", IsRequired=true, DefaultValue=""), StringValidator]
        public string Password
        {
            get
            {
                return (string) base[_propPassword];
            }
            set
            {
                base[_propPassword] = value;
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

