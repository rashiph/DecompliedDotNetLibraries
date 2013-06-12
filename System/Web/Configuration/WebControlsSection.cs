namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;

    public sealed class WebControlsSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propClientScriptsLocation = new ConfigurationProperty("clientScriptsLocation", typeof(string), "/aspnet_client/{0}/{1}/", null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        static WebControlsSection()
        {
            _properties.Add(_propClientScriptsLocation);
        }

        protected override object GetRuntimeObject()
        {
            Hashtable hashtable = new Hashtable();
            foreach (ConfigurationProperty property in this.Properties)
            {
                hashtable[property.Name] = base[property];
            }
            return hashtable;
        }

        [ConfigurationProperty("clientScriptsLocation", IsRequired=true, DefaultValue="/aspnet_client/{0}/{1}/"), StringValidator(MinLength=1)]
        public string ClientScriptsLocation
        {
            get
            {
                return (string) base[_propClientScriptsLocation];
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

