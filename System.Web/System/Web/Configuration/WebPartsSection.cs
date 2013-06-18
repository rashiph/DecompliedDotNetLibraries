namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class WebPartsSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty _propEnableExport = new ConfigurationProperty("enableExport", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPersonalization = new ConfigurationProperty("personalization", typeof(WebPartsPersonalization), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propTransformers = new ConfigurationProperty("transformers", typeof(TransformerInfoCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static WebPartsSection()
        {
            _properties.Add(_propEnableExport);
            _properties.Add(_propPersonalization);
            _properties.Add(_propTransformers);
        }

        protected override object GetRuntimeObject()
        {
            this.Personalization.ValidateAuthorization();
            return base.GetRuntimeObject();
        }

        [ConfigurationProperty("enableExport", DefaultValue=false)]
        public bool EnableExport
        {
            get
            {
                return (bool) base[_propEnableExport];
            }
            set
            {
                base[_propEnableExport] = value;
            }
        }

        [ConfigurationProperty("personalization")]
        public WebPartsPersonalization Personalization
        {
            get
            {
                return (WebPartsPersonalization) base[_propPersonalization];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("transformers")]
        public TransformerInfoCollection Transformers
        {
            get
            {
                return (TransformerInfoCollection) base[_propTransformers];
            }
        }
    }
}

