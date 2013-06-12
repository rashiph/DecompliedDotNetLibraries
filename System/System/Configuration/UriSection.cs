namespace System.Configuration
{
    using System;

    public sealed class UriSection : ConfigurationSection
    {
        private static readonly ConfigurationProperty idn = new ConfigurationProperty("idn", typeof(IdnElement), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty iriParsing = new ConfigurationProperty("iriParsing", typeof(IriParsingElement), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty schemeSettings = new ConfigurationProperty("schemeSettings", typeof(SchemeSettingElementCollection), null, ConfigurationPropertyOptions.None);

        static UriSection()
        {
            properties.Add(idn);
            properties.Add(iriParsing);
            properties.Add(schemeSettings);
        }

        [ConfigurationProperty("idn")]
        public IdnElement Idn
        {
            get
            {
                return (IdnElement) base[idn];
            }
        }

        [ConfigurationProperty("iriParsing")]
        public IriParsingElement IriParsing
        {
            get
            {
                return (IriParsingElement) base[iriParsing];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        [ConfigurationProperty("schemeSettings")]
        public SchemeSettingElementCollection SchemeSettings
        {
            get
            {
                return (SchemeSettingElementCollection) base[schemeSettings];
            }
        }
    }
}

