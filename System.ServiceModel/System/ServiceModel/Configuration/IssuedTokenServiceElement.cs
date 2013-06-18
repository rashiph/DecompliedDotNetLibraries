namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    public sealed class IssuedTokenServiceElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(IssuedTokenServiceCredential issuedToken)
        {
            if (issuedToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuedToken");
            }
            issuedToken.CertificateValidationMode = this.CertificateValidationMode;
            issuedToken.RevocationMode = this.RevocationMode;
            issuedToken.TrustedStoreLocation = this.TrustedStoreLocation;
            issuedToken.AudienceUriMode = this.AudienceUriMode;
            if (!string.IsNullOrEmpty(this.CustomCertificateValidatorType))
            {
                Type c = Type.GetType(this.CustomCertificateValidatorType, true);
                if (!typeof(X509CertificateValidator).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCertificateValidatorType", new object[] { this.CustomCertificateValidatorType, typeof(X509CertificateValidator).ToString() })));
                }
                issuedToken.CustomCertificateValidator = (X509CertificateValidator) Activator.CreateInstance(c);
            }
            if (!string.IsNullOrEmpty(this.SamlSerializerType))
            {
                Type type = Type.GetType(this.SamlSerializerType, true);
                if (!typeof(SamlSerializer).IsAssignableFrom(type))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidSamlSerializerType", new object[] { this.SamlSerializerType, typeof(SamlSerializer).ToString() })));
                }
                issuedToken.SamlSerializer = (SamlSerializer) Activator.CreateInstance(type);
            }
            PropertyInformationCollection properties = base.ElementInformation.Properties;
            if (properties["knownCertificates"].ValueOrigin != PropertyValueOrigin.Default)
            {
                foreach (X509CertificateTrustedIssuerElement element in this.KnownCertificates)
                {
                    issuedToken.KnownCertificates.Add(System.ServiceModel.Security.SecurityUtils.GetCertificateFromStore(element.StoreName, element.StoreLocation, element.X509FindType, element.FindValue, null));
                }
            }
            if (properties["allowedAudienceUris"].ValueOrigin != PropertyValueOrigin.Default)
            {
                foreach (AllowedAudienceUriElement element2 in this.AllowedAudienceUris)
                {
                    issuedToken.AllowedAudienceUris.Add(element2.AllowedAudienceUri);
                }
            }
            issuedToken.AllowUntrustedRsaIssuers = this.AllowUntrustedRsaIssuers;
        }

        public void Copy(IssuedTokenServiceElement from)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (from == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("from");
            }
            this.SamlSerializerType = from.SamlSerializerType;
            PropertyInformationCollection properties = from.ElementInformation.Properties;
            if (properties["knownCertificates"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.KnownCertificates.Clear();
                foreach (X509CertificateTrustedIssuerElement element in from.KnownCertificates)
                {
                    X509CertificateTrustedIssuerElement element2 = new X509CertificateTrustedIssuerElement();
                    element2.Copy(element);
                    this.KnownCertificates.Add(element2);
                }
            }
            if (properties["allowedAudienceUris"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.AllowedAudienceUris.Clear();
                foreach (AllowedAudienceUriElement element3 in from.AllowedAudienceUris)
                {
                    AllowedAudienceUriElement element4 = new AllowedAudienceUriElement {
                        AllowedAudienceUri = element3.AllowedAudienceUri
                    };
                    this.AllowedAudienceUris.Add(element4);
                }
            }
            this.AllowUntrustedRsaIssuers = from.AllowUntrustedRsaIssuers;
            this.CertificateValidationMode = from.CertificateValidationMode;
            this.AudienceUriMode = from.AudienceUriMode;
            this.CustomCertificateValidatorType = from.CustomCertificateValidatorType;
            this.RevocationMode = from.RevocationMode;
            this.TrustedStoreLocation = from.TrustedStoreLocation;
        }

        [ConfigurationProperty("allowedAudienceUris")]
        public AllowedAudienceUriElementCollection AllowedAudienceUris
        {
            get
            {
                return (AllowedAudienceUriElementCollection) base["allowedAudienceUris"];
            }
        }

        [ConfigurationProperty("allowUntrustedRsaIssuers", DefaultValue=false)]
        public bool AllowUntrustedRsaIssuers
        {
            get
            {
                return (bool) base["allowUntrustedRsaIssuers"];
            }
            set
            {
                base["allowUntrustedRsaIssuers"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(AudienceUriModeValidationHelper)), ConfigurationProperty("audienceUriMode", DefaultValue=1)]
        public System.IdentityModel.Selectors.AudienceUriMode AudienceUriMode
        {
            get
            {
                return (System.IdentityModel.Selectors.AudienceUriMode) base["audienceUriMode"];
            }
            set
            {
                base["audienceUriMode"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(X509CertificateValidationModeHelper)), ConfigurationProperty("certificateValidationMode", DefaultValue=2)]
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

        [StringValidator(MinLength=0), ConfigurationProperty("customCertificateValidatorType", DefaultValue="")]
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

        [ConfigurationProperty("knownCertificates")]
        public X509CertificateTrustedIssuerElementCollection KnownCertificates
        {
            get
            {
                return (X509CertificateTrustedIssuerElementCollection) base["knownCertificates"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("allowedAudienceUris", typeof(AllowedAudienceUriElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("audienceUriMode", typeof(System.IdentityModel.Selectors.AudienceUriMode), System.IdentityModel.Selectors.AudienceUriMode.Always, null, new ServiceModelEnumValidator(typeof(AudienceUriModeValidationHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("customCertificateValidatorType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("certificateValidationMode", typeof(X509CertificateValidationMode), X509CertificateValidationMode.ChainTrust, null, new ServiceModelEnumValidator(typeof(X509CertificateValidationModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("revocationMode", typeof(X509RevocationMode), X509RevocationMode.Online, null, new StandardRuntimeEnumValidator(typeof(X509RevocationMode)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("trustedStoreLocation", typeof(StoreLocation), StoreLocation.LocalMachine, null, new StandardRuntimeEnumValidator(typeof(StoreLocation)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("samlSerializerType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("knownCertificates", typeof(X509CertificateTrustedIssuerElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("allowUntrustedRsaIssuers", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StandardRuntimeEnumValidator(typeof(X509RevocationMode)), ConfigurationProperty("revocationMode", DefaultValue=1)]
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

        [StringValidator(MinLength=0), ConfigurationProperty("samlSerializerType", DefaultValue="")]
        public string SamlSerializerType
        {
            get
            {
                return (string) base["samlSerializerType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["samlSerializerType"] = value;
            }
        }

        [ConfigurationProperty("trustedStoreLocation", DefaultValue=2), StandardRuntimeEnumValidator(typeof(StoreLocation))]
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

