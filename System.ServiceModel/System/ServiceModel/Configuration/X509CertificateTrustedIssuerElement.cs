namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public sealed class X509CertificateTrustedIssuerElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        public void Copy(X509CertificateTrustedIssuerElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.FindValue = from.FindValue;
            this.StoreLocation = from.StoreLocation;
            this.StoreName = from.StoreName;
            this.X509FindType = from.X509FindType;
        }

        [StringValidator(MinLength=0), ConfigurationProperty("findValue", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("findValue", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), ConfigurationProperty("storeLocation", DefaultValue=2, Options=ConfigurationPropertyOptions.IsKey)]
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

        [ConfigurationProperty("storeName", DefaultValue=5, Options=ConfigurationPropertyOptions.IsKey), StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName))]
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

        [StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), ConfigurationProperty("x509FindType", DefaultValue=2, Options=ConfigurationPropertyOptions.IsKey)]
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

