namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.IdentityModel.Selectors;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class X509ClientCertificateAuthenticationElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(X509ClientCertificateAuthentication cert)
        {
            if (cert == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cert");
            }
            cert.CertificateValidationMode = this.CertificateValidationMode;
            cert.RevocationMode = this.RevocationMode;
            cert.TrustedStoreLocation = this.TrustedStoreLocation;
            cert.IncludeWindowsGroups = this.IncludeWindowsGroups;
            cert.MapClientCertificateToWindowsAccount = this.MapClientCertificateToWindowsAccount;
            if (!string.IsNullOrEmpty(this.CustomCertificateValidatorType))
            {
                Type c = Type.GetType(this.CustomCertificateValidatorType, true);
                if (!typeof(X509CertificateValidator).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCertificateValidatorType", new object[] { this.CustomCertificateValidatorType, typeof(X509CertificateValidator).ToString() })));
                }
                cert.CustomCertificateValidator = (X509CertificateValidator) Activator.CreateInstance(c);
            }
        }

        public void Copy(X509ClientCertificateAuthenticationElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.CertificateValidationMode = from.CertificateValidationMode;
            this.RevocationMode = from.RevocationMode;
            this.TrustedStoreLocation = from.TrustedStoreLocation;
            this.IncludeWindowsGroups = from.IncludeWindowsGroups;
            this.MapClientCertificateToWindowsAccount = from.MapClientCertificateToWindowsAccount;
            this.CustomCertificateValidatorType = from.CustomCertificateValidatorType;
        }

        [ConfigurationProperty("certificateValidationMode", DefaultValue=2), ServiceModelEnumValidator(typeof(X509CertificateValidationModeHelper))]
        public X509CertificateValidationMode CertificateValidationMode
        {
            get
            {
                return (X509CertificateValidationMode) base["certificateValidationMode"];
            }
            set
            {
                base["certificateValidationMode"] = value;
            }
        }

        [ConfigurationProperty("customCertificateValidatorType", DefaultValue=""), StringValidator(MinLength=0)]
        public string CustomCertificateValidatorType
        {
            get
            {
                return (string) base["customCertificateValidatorType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["customCertificateValidatorType"] = value;
            }
        }

        [ConfigurationProperty("includeWindowsGroups", DefaultValue=true)]
        public bool IncludeWindowsGroups
        {
            get
            {
                return (bool) base["includeWindowsGroups"];
            }
            set
            {
                base["includeWindowsGroups"] = value;
            }
        }

        [ConfigurationProperty("mapClientCertificateToWindowsAccount", DefaultValue=false)]
        public bool MapClientCertificateToWindowsAccount
        {
            get
            {
                return (bool) base["mapClientCertificateToWindowsAccount"];
            }
            set
            {
                base["mapClientCertificateToWindowsAccount"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("certificateValidationMode", typeof(X509CertificateValidationMode), X509CertificateValidationMode.ChainTrust, null, new ServiceModelEnumValidator(typeof(X509CertificateValidationModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("revocationMode", typeof(X509RevocationMode), X509RevocationMode.Online, null, new StandardRuntimeEnumValidator(typeof(X509RevocationMode)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("trustedStoreLocation", typeof(StoreLocation), StoreLocation.LocalMachine, null, new StandardRuntimeEnumValidator(typeof(StoreLocation)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("includeWindowsGroups", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("mapClientCertificateToWindowsAccount", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("revocationMode", DefaultValue=1), StandardRuntimeEnumValidator(typeof(X509RevocationMode))]
        public X509RevocationMode RevocationMode
        {
            get
            {
                return (X509RevocationMode) base["revocationMode"];
            }
            set
            {
                base["revocationMode"] = value;
            }
        }

        [StandardRuntimeEnumValidator(typeof(StoreLocation)), ConfigurationProperty("trustedStoreLocation", DefaultValue=2)]
        public StoreLocation TrustedStoreLocation
        {
            get
            {
                return (StoreLocation) base["trustedStoreLocation"];
            }
            set
            {
                base["trustedStoreLocation"] = value;
            }
        }
    }
}

