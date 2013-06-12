namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class PartialTrustVisibleAssembly : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAssemblyName = new ConfigurationProperty("assemblyName", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPublicKey = new ConfigurationProperty("publicKey", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);

        static PartialTrustVisibleAssembly()
        {
            _properties.Add(_propAssemblyName);
            _properties.Add(_propPublicKey);
        }

        internal PartialTrustVisibleAssembly()
        {
        }

        public PartialTrustVisibleAssembly(string assemblyName, string publicKey)
        {
            this.AssemblyName = assemblyName;
            this.PublicKey = publicKey;
        }

        [ConfigurationProperty("assemblyName", IsRequired=true, IsKey=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string AssemblyName
        {
            get
            {
                return (string) base[_propAssemblyName];
            }
            set
            {
                base[_propAssemblyName] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [StringValidator(MinLength=1), ConfigurationProperty("publicKey", IsRequired=true, IsKey=false, DefaultValue="")]
        public string PublicKey
        {
            get
            {
                return (string) base[_propPublicKey];
            }
            set
            {
                base[_propPublicKey] = value;
            }
        }
    }
}

