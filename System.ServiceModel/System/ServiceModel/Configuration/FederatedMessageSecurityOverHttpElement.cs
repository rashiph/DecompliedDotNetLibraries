namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Tokens;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    public sealed class FederatedMessageSecurityOverHttpElement : ConfigurationElement
    {
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(FederatedMessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            security.NegotiateServiceCredential = this.NegotiateServiceCredential;
            security.AlgorithmSuite = this.AlgorithmSuite;
            security.IssuedKeyType = this.IssuedKeyType;
            security.EstablishSecurityContext = this.EstablishSecurityContext;
            if (!string.IsNullOrEmpty(this.IssuedTokenType))
            {
                security.IssuedTokenType = this.IssuedTokenType;
            }
            if (base.ElementInformation.Properties["issuer"].ValueOrigin != PropertyValueOrigin.Default)
            {
                security.IssuerAddress = ConfigLoader.LoadEndpointAddress(this.Issuer);
                if (!string.IsNullOrEmpty(this.Issuer.Binding))
                {
                    security.IssuerBinding = ConfigLoader.LookupBinding(this.Issuer.Binding, this.Issuer.BindingConfiguration, base.EvaluationContext);
                }
            }
            if (base.ElementInformation.Properties["issuerMetadata"].ValueOrigin != PropertyValueOrigin.Default)
            {
                security.IssuerMetadataAddress = ConfigLoader.LoadEndpointAddress(this.IssuerMetadata);
            }
            foreach (XmlElementElement element in this.TokenRequestParameters)
            {
                security.TokenRequestParameters.Add(element.XmlElement);
            }
            foreach (ClaimTypeElement element2 in this.ClaimTypeRequirements)
            {
                security.ClaimTypeRequirements.Add(new ClaimTypeRequirement(element2.ClaimType, element2.IsOptional));
            }
        }

        internal void InitializeFrom(FederatedMessageSecurityOverHttp security)
        {
            if (security == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("security");
            }
            this.NegotiateServiceCredential = security.NegotiateServiceCredential;
            this.AlgorithmSuite = security.AlgorithmSuite;
            this.IssuedKeyType = security.IssuedKeyType;
            if (!security.EstablishSecurityContext)
            {
                this.EstablishSecurityContext = security.EstablishSecurityContext;
            }
            if (security.IssuedTokenType != null)
            {
                this.IssuedTokenType = security.IssuedTokenType;
            }
            if (security.IssuerAddress != null)
            {
                this.Issuer.InitializeFrom(security.IssuerAddress);
            }
            if (security.IssuerMetadataAddress != null)
            {
                this.IssuerMetadata.InitializeFrom(security.IssuerMetadataAddress);
            }
            string bindingSectionName = null;
            if (security.IssuerBinding != null)
            {
                if (null == this.Issuer.Address)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigNullIssuerAddress")));
                }
                this.Issuer.BindingConfiguration = this.Issuer.Address.ToString();
                BindingsSection.TryAdd(this.Issuer.BindingConfiguration, security.IssuerBinding, out bindingSectionName);
                this.Issuer.Binding = bindingSectionName;
            }
            foreach (XmlElement element in security.TokenRequestParameters)
            {
                this.TokenRequestParameters.Add(new XmlElementElement(element));
            }
            foreach (ClaimTypeRequirement requirement in security.ClaimTypeRequirements)
            {
                ClaimTypeElement element2 = new ClaimTypeElement(requirement.ClaimType, requirement.IsOptional);
                this.ClaimTypeRequirements.Add(element2);
            }
        }

        [ConfigurationProperty("algorithmSuite", DefaultValue="Default"), TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
        public SecurityAlgorithmSuite AlgorithmSuite
        {
            get
            {
                return (SecurityAlgorithmSuite) base["algorithmSuite"];
            }
            set
            {
                base["algorithmSuite"] = value;
            }
        }

        [ConfigurationProperty("claimTypeRequirements")]
        public ClaimTypeElementCollection ClaimTypeRequirements
        {
            get
            {
                return (ClaimTypeElementCollection) base["claimTypeRequirements"];
            }
        }

        [ConfigurationProperty("establishSecurityContext", DefaultValue=true)]
        public bool EstablishSecurityContext
        {
            get
            {
                return (bool) base["establishSecurityContext"];
            }
            set
            {
                base["establishSecurityContext"] = value;
            }
        }

        [ServiceModelEnumValidator(typeof(SecurityKeyTypeHelper)), ConfigurationProperty("issuedKeyType", DefaultValue=0)]
        public SecurityKeyType IssuedKeyType
        {
            get
            {
                return (SecurityKeyType) base["issuedKeyType"];
            }
            set
            {
                base["issuedKeyType"] = value;
            }
        }

        [ConfigurationProperty("issuedTokenType", DefaultValue=""), StringValidator(MinLength=0)]
        public string IssuedTokenType
        {
            get
            {
                return (string) base["issuedTokenType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["issuedTokenType"] = value;
            }
        }

        [ConfigurationProperty("issuer")]
        public IssuedTokenParametersEndpointAddressElement Issuer
        {
            get
            {
                return (IssuedTokenParametersEndpointAddressElement) base["issuer"];
            }
        }

        [ConfigurationProperty("issuerMetadata")]
        public EndpointAddressElementBase IssuerMetadata
        {
            get
            {
                return (EndpointAddressElementBase) base["issuerMetadata"];
            }
        }

        [ConfigurationProperty("negotiateServiceCredential", DefaultValue=true)]
        public bool NegotiateServiceCredential
        {
            get
            {
                return (bool) base["negotiateServiceCredential"];
            }
            set
            {
                base["negotiateServiceCredential"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("algorithmSuite", typeof(SecurityAlgorithmSuite), "Default", new SecurityAlgorithmSuiteConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("claimTypeRequirements", typeof(ClaimTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("establishSecurityContext", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuedKeyType", typeof(SecurityKeyType), SecurityKeyType.SymmetricKey, null, new ServiceModelEnumValidator(typeof(SecurityKeyTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuedTokenType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuer", typeof(IssuedTokenParametersEndpointAddressElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuerMetadata", typeof(EndpointAddressElementBase), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("negotiateServiceCredential", typeof(bool), true, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("tokenRequestParameters", typeof(XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("tokenRequestParameters")]
        public XmlElementElementCollection TokenRequestParameters
        {
            get
            {
                return (XmlElementElementCollection) base["tokenRequestParameters"];
            }
        }
    }
}

