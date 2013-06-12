namespace System.Configuration
{
    using System;

    public sealed class SchemeSettingElement : ConfigurationElement
    {
        private static readonly ConfigurationProperty genericUriParserOptions = new ConfigurationProperty("genericUriParserOptions", typeof(System.GenericUriParserOptions), System.GenericUriParserOptions.Default, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        static SchemeSettingElement()
        {
            properties.Add(name);
            properties.Add(genericUriParserOptions);
        }

        [ConfigurationProperty("genericUriParserOptions", DefaultValue=0, IsRequired=true)]
        public System.GenericUriParserOptions GenericUriParserOptions
        {
            get
            {
                return (System.GenericUriParserOptions) base[genericUriParserOptions];
            }
        }

        [ConfigurationProperty("name", DefaultValue=null, IsRequired=true, IsKey=true)]
        public string Name
        {
            get
            {
                return (string) base[name];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }
    }
}

