namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;

    public sealed class CertificateReferenceElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        [StringValidator(MinLength=0), ConfigurationProperty("findValue", DefaultValue="")]
        public string FindValue
        {
            get
            {
                return (string) base["findValue"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["findValue"] = value;
            }
        }

        [ConfigurationProperty("isChainIncluded", DefaultValue=false)]
        public bool IsChainIncluded
        {
            get
            {
                return (bool) base["isChainIncluded"];
            }
            set
            {
                base["isChainIncluded"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("findValue", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("isChainIncluded", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("storeLocation", DefaultValue=2), StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation))]
        public System.Security.Cryptography.X509Certificates.StoreLocation StoreLocation
        {
            get
            {
                return (System.Security.Cryptography.X509Certificates.StoreLocation) base["storeLocation"];
            }
            set
            {
                base["storeLocation"] = value;
            }
        }

        [ConfigurationProperty("storeName", DefaultValue=5), StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName))]
        public System.Security.Cryptography.X509Certificates.StoreName StoreName
        {
            get
            {
                return (System.Security.Cryptography.X509Certificates.StoreName) base["storeName"];
            }
            set
            {
                base["storeName"] = value;
            }
        }

        [ConfigurationProperty("x509FindType", DefaultValue=2), StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType))]
        public System.Security.Cryptography.X509Certificates.X509FindType X509FindType
        {
            get
            {
                return (System.Security.Cryptography.X509Certificates.X509FindType) base["x509FindType"];
            }
            set
            {
                base["x509FindType"] = value;
            }
        }
    }
}

