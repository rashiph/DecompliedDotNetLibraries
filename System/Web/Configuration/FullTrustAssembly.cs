namespace System.Web.Configuration
{
    using System;
    using System.Configuration;

    public sealed class FullTrustAssembly : ConfigurationElement
    {
        private static readonly ConfigurationProperty _propAssemblyName = new ConfigurationProperty("assemblyName", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPublicKey = new ConfigurationProperty("publicKey", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propVersion = new ConfigurationProperty("version", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);

        static FullTrustAssembly()
        {
            _properties.Add(_propAssemblyName);
            _properties.Add(_propVersion);
            _properties.Add(_propPublicKey);
        }

        internal FullTrustAssembly()
        {
        }

        public FullTrustAssembly(string assemblyName, string version, string publicKey)
        {
            this.AssemblyName = assemblyName;
            this.Version = version;
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

        [ConfigurationProperty("publicKey", IsRequired=true, IsKey=false, DefaultValue=""), StringValidator(MinLength=1)]
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

        [ConfigurationProperty("version", IsRequired=true, IsKey=true, DefaultValue=""), StringValidator(MinLength=1)]
        public string Version
        {
            get
            {
                return (string) base[_propVersion];
            }
            set
            {
                base[_propVersion] = value;
            }
        }
    }
}

