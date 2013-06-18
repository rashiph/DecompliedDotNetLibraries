namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Configuration;
    using System.IdentityModel.Tokens;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security.Tokens;
    using System.Text;
    using System.Xml;

    public sealed class IssuedTokenParametersElement : ConfigurationElement
    {
        private Collection<IssuedTokenParametersElement> optionalIssuedTokenParameters;
        private ConfigurationPropertyCollection properties;

        internal void ApplyConfiguration(IssuedSecurityTokenParameters parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));
            }
            if (this.AdditionalRequestParameters != null)
            {
                foreach (XmlElementElement element in this.AdditionalRequestParameters)
                {
                    parameters.AdditionalRequestParameters.Add(element.XmlElement);
                }
            }
            if (this.ClaimTypeRequirements != null)
            {
                foreach (ClaimTypeElement element2 in this.ClaimTypeRequirements)
                {
                    parameters.ClaimTypeRequirements.Add(new ClaimTypeRequirement(element2.ClaimType, element2.IsOptional));
                }
            }
            parameters.KeySize = this.KeySize;
            parameters.KeyType = this.KeyType;
            parameters.DefaultMessageSecurityVersion = this.DefaultMessageSecurityVersion;
            if (!string.IsNullOrEmpty(this.TokenType))
            {
                parameters.TokenType = this.TokenType;
            }
            if (base.ElementInformation.Properties["issuer"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Issuer.Validate();
                parameters.IssuerAddress = ConfigLoader.LoadEndpointAddress(this.Issuer);
                if (!string.IsNullOrEmpty(this.Issuer.Binding))
                {
                    parameters.IssuerBinding = ConfigLoader.LookupBinding(this.Issuer.Binding, this.Issuer.BindingConfiguration, base.EvaluationContext);
                }
            }
            if (base.ElementInformation.Properties["issuerMetadata"].ValueOrigin != PropertyValueOrigin.Default)
            {
                parameters.IssuerMetadataAddress = ConfigLoader.LoadEndpointAddress(this.IssuerMetadata);
            }
        }

        internal void Copy(IssuedTokenParametersElement source)
        {
            if (this.IsReadOnly())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigReadOnly")));
            }
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            foreach (XmlElementElement element in source.AdditionalRequestParameters)
            {
                XmlElementElement element2 = new XmlElementElement();
                element2.Copy(element);
                this.AdditionalRequestParameters.Add(element2);
            }
            foreach (ClaimTypeElement element3 in source.ClaimTypeRequirements)
            {
                this.ClaimTypeRequirements.Add(new ClaimTypeElement(element3.ClaimType, element3.IsOptional));
            }
            this.KeySize = source.KeySize;
            this.KeyType = source.KeyType;
            this.TokenType = source.TokenType;
            this.DefaultMessageSecurityVersion = source.DefaultMessageSecurityVersion;
            if (source.ElementInformation.Properties["issuer"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Issuer.Copy(source.Issuer);
            }
            if (source.ElementInformation.Properties["issuerMetadata"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IssuerMetadata.Copy(source.IssuerMetadata);
            }
        }

        internal IssuedSecurityTokenParameters Create(bool createTemplateOnly, SecurityKeyType templateKeyType)
        {
            IssuedSecurityTokenParameters parameters = new IssuedSecurityTokenParameters();
            if (!createTemplateOnly)
            {
                this.ApplyConfiguration(parameters);
                return parameters;
            }
            parameters.KeyType = templateKeyType;
            return parameters;
        }

        internal void InitializeFrom(IssuedSecurityTokenParameters source, bool initializeNestedBindings)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            this.KeyType = source.KeyType;
            if (source.KeySize > 0)
            {
                this.KeySize = source.KeySize;
            }
            this.TokenType = source.TokenType;
            if (source.IssuerAddress != null)
            {
                this.Issuer.InitializeFrom(source.IssuerAddress);
            }
            if (source.DefaultMessageSecurityVersion != null)
            {
                this.DefaultMessageSecurityVersion = source.DefaultMessageSecurityVersion;
            }
            if ((source.IssuerBinding != null) && initializeNestedBindings)
            {
                string str;
                this.Issuer.BindingConfiguration = this.Issuer.Address.ToString();
                BindingsSection.TryAdd(this.Issuer.BindingConfiguration, source.IssuerBinding, out str);
                this.Issuer.Binding = str;
            }
            if (source.IssuerMetadataAddress != null)
            {
                this.IssuerMetadata.InitializeFrom(source.IssuerMetadataAddress);
            }
            foreach (XmlElement element in source.AdditionalRequestParameters)
            {
                this.AdditionalRequestParameters.Add(new XmlElementElement(element));
            }
            foreach (ClaimTypeRequirement requirement in source.ClaimTypeRequirements)
            {
                this.ClaimTypeRequirements.Add(new ClaimTypeElement(requirement.ClaimType, requirement.IsOptional));
            }
            foreach (IssuedSecurityTokenParameters.AlternativeIssuerEndpoint endpoint in source.AlternativeIssuerEndpoints)
            {
                IssuedTokenParametersElement item = new IssuedTokenParametersElement();
                item.Issuer.InitializeFrom(endpoint.IssuerAddress);
                if (initializeNestedBindings)
                {
                    string str2;
                    item.Issuer.BindingConfiguration = item.Issuer.Address.ToString();
                    BindingsSection.TryAdd(item.Issuer.BindingConfiguration, endpoint.IssuerBinding, out str2);
                    item.Issuer.Binding = str2;
                }
                this.OptionalIssuedTokenParameters.Add(item);
            }
        }

        protected override bool SerializeToXmlElement(XmlWriter writer, string elementName)
        {
            bool flag = base.SerializeToXmlElement(writer, elementName);
            bool flag2 = this.OptionalIssuedTokenParameters.Count > 0;
            if (flag2 && (writer != null))
            {
                MemoryStream w = new MemoryStream();
                using (XmlTextWriter writer2 = new XmlTextWriter(w, Encoding.UTF8))
                {
                    writer2.Formatting = Formatting.Indented;
                    writer2.WriteStartElement("alternativeIssuedTokenParameters");
                    foreach (IssuedTokenParametersElement element in this.OptionalIssuedTokenParameters)
                    {
                        element.SerializeToXmlElement(writer2, "issuedTokenParameters");
                    }
                    writer2.WriteEndElement();
                    writer2.Flush();
                    string str = new UTF8Encoding().GetString(w.GetBuffer(), 0, (int) w.Length);
                    writer.WriteComment(str.Substring(1, str.Length - 1));
                    writer2.Close();
                }
            }
            if (!flag)
            {
                return flag2;
            }
            return true;
        }

        protected override void Unmerge(ConfigurationElement sourceElement, ConfigurationElement parentElement, ConfigurationSaveMode saveMode)
        {
            if (sourceElement is IssuedTokenParametersElement)
            {
                IssuedTokenParametersElement element = (IssuedTokenParametersElement) sourceElement;
                this.optionalIssuedTokenParameters = element.optionalIssuedTokenParameters;
            }
            base.Unmerge(sourceElement, parentElement, saveMode);
        }

        [ConfigurationProperty("additionalRequestParameters")]
        public XmlElementElementCollection AdditionalRequestParameters
        {
            get
            {
                return (XmlElementElementCollection) base["additionalRequestParameters"];
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

        [TypeConverter(typeof(MessageSecurityVersionConverter)), ConfigurationProperty("defaultMessageSecurityVersion")]
        public MessageSecurityVersion DefaultMessageSecurityVersion
        {
            get
            {
                return (MessageSecurityVersion) base["defaultMessageSecurityVersion"];
            }
            set
            {
                base["defaultMessageSecurityVersion"] = value;
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

        [IntegerValidator(MinValue=0), ConfigurationProperty("keySize", DefaultValue=0)]
        public int KeySize
        {
            get
            {
                return (int) base["keySize"];
            }
            set
            {
                base["keySize"] = value;
            }
        }

        [ConfigurationProperty("keyType", DefaultValue=0), ServiceModelEnumValidator(typeof(SecurityKeyTypeHelper))]
        public SecurityKeyType KeyType
        {
            get
            {
                return (SecurityKeyType) base["keyType"];
            }
            set
            {
                base["keyType"] = value;
            }
        }

        internal Collection<IssuedTokenParametersElement> OptionalIssuedTokenParameters
        {
            get
            {
                if (this.IsReadOnly())
                {
                    DiagnosticUtility.FailFast("IssuedTokenParametersElement.OptionalIssuedTokenParameters should only be called by Admin APIs");
                }
                if (this.optionalIssuedTokenParameters == null)
                {
                    this.optionalIssuedTokenParameters = new Collection<IssuedTokenParametersElement>();
                }
                return this.optionalIssuedTokenParameters;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("defaultMessageSecurityVersion", typeof(MessageSecurityVersion), null, new MessageSecurityVersionConverter(), null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("additionalRequestParameters", typeof(XmlElementElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("claimTypeRequirements", typeof(ClaimTypeElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuer", typeof(IssuedTokenParametersEndpointAddressElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("issuerMetadata", typeof(EndpointAddressElementBase), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("keySize", typeof(int), 0, null, new IntegerValidator(0, 0x7fffffff, false), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("keyType", typeof(SecurityKeyType), SecurityKeyType.SymmetricKey, null, new ServiceModelEnumValidator(typeof(SecurityKeyTypeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("tokenType", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("tokenType", DefaultValue="")]
        public string TokenType
        {
            get
            {
                return (string) base["tokenType"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["tokenType"] = value;
            }
        }
    }
}

