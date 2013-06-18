namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class X509ScopedServiceCertificateElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(X509CertificateRecipientClientCredential creds)
        {
            if (creds == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("creds");
            }
            PropertyInformationCollection properties = base.ElementInformation.Properties;
            if (((properties["targetUri"].ValueOrigin != PropertyValueOrigin.Default) || (properties["storeLocation"].ValueOrigin != PropertyValueOrigin.Default)) || (((properties["storeName"].ValueOrigin != PropertyValueOrigin.Default) || (properties["x509FindType"].ValueOrigin != PropertyValueOrigin.Default)) || (properties["findValue"].ValueOrigin != PropertyValueOrigin.Default)))
            {
                creds.SetScopedCertificate(this.StoreLocation, this.StoreName, this.X509FindType, this.FindValue, this.TargetUri);
            }
        }

        public void Copy(X509ScopedServiceCertificateElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.TargetUri = from.TargetUri;
            this.FindValue = from.FindValue;
            this.StoreLocation = from.StoreLocation;
            this.StoreName = from.StoreName;
            this.X509FindType = from.X509FindType;
        }

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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("targetUri", typeof(Uri), null, null, null, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired));
                    propertys.Add(new ConfigurationProperty("findValue", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("storeLocation", typeof(System.Security.Cryptography.X509Certificates.StoreLocation), System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("storeName", typeof(System.Security.Cryptography.X509Certificates.StoreName), System.Security.Cryptography.X509Certificates.StoreName.My, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("x509FindType", typeof(System.Security.Cryptography.X509Certificates.X509FindType), System.Security.Cryptography.X509Certificates.X509FindType.FindBySubjectDistinguishedName, null, new StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.X509FindType)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreLocation)), ConfigurationProperty("storeLocation", DefaultValue=1)]
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

        [StandardRuntimeEnumValidator(typeof(System.Security.Cryptography.X509Certificates.StoreName)), ConfigurationProperty("storeName", DefaultValue=5)]
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

        [ConfigurationProperty("targetUri", DefaultValue=null, Options=ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired)]
        public Uri TargetUri
        {
            get
            {
                return (Uri) base["targetUri"];
            }
            set
            {
                base["targetUri"] = value;
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

