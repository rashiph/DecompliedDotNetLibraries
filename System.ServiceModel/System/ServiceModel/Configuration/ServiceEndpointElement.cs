namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public sealed class ServiceEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        private ConfigurationPropertyCollection properties;

        public ServiceEndpointElement()
        {
        }

        public ServiceEndpointElement(Uri address, string contractType) : this()
        {
            this.Address = address;
            this.Contract = contractType;
        }

        internal void Copy(ServiceEndpointElement source)
        {
            if (source == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("source");
            }
            PropertyInformationCollection properties = source.ElementInformation.Properties;
            if (properties["address"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Address = source.Address;
            }
            if (properties["behaviorConfiguration"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BehaviorConfiguration = source.BehaviorConfiguration;
            }
            if (properties["binding"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Binding = source.Binding;
            }
            if (properties["bindingConfiguration"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingConfiguration = source.BindingConfiguration;
            }
            if (properties["name"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Name = source.Name;
            }
            if (properties["bindingName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingName = source.BindingName;
            }
            if (properties["bindingNamespace"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.BindingNamespace = source.BindingNamespace;
            }
            if (properties["contract"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Contract = source.Contract;
            }
            if ((properties["headers"].ValueOrigin != PropertyValueOrigin.Default) && (source.Headers != null))
            {
                this.Headers.Copy(source.Headers);
            }
            if ((properties["identity"].ValueOrigin != PropertyValueOrigin.Default) && (source.Identity != null))
            {
                this.Identity.Copy(source.Identity);
            }
            if (properties["listenUriMode"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ListenUriMode = source.ListenUriMode;
            }
            if (properties["listenUri"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ListenUri = source.ListenUri;
            }
            if (properties["isSystemEndpoint"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IsSystemEndpoint = source.IsSystemEndpoint;
            }
            if (properties["kind"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Kind = source.Kind;
            }
            if (properties["endpointConfiguration"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.EndpointConfiguration = source.EndpointConfiguration;
            }
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return null;
        }

        [ConfigurationProperty("address", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
        public Uri Address
        {
            get
            {
                return (Uri) base["address"];
            }
            set
            {
                base["address"] = value;
            }
        }

        [ConfigurationProperty("behaviorConfiguration", DefaultValue=""), StringValidator(MinLength=0)]
        public string BehaviorConfiguration
        {
            get
            {
                return (string) base["behaviorConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["behaviorConfiguration"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("binding", Options=ConfigurationPropertyOptions.IsKey)]
        public string Binding
        {
            get
            {
                return (string) base["binding"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["binding"] = value;
            }
        }

        [ConfigurationProperty("bindingConfiguration", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
        public string BindingConfiguration
        {
            get
            {
                return (string) base["bindingConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["bindingConfiguration"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("bindingName", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
        public string BindingName
        {
            get
            {
                return (string) base["bindingName"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["bindingName"] = value;
            }
        }

        [ConfigurationProperty("bindingNamespace", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
        public string BindingNamespace
        {
            get
            {
                return (string) base["bindingNamespace"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["bindingNamespace"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("contract", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
        public string Contract
        {
            get
            {
                return (string) base["contract"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["contract"] = value;
            }
        }

        [ConfigurationProperty("endpointConfiguration", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
        public string EndpointConfiguration
        {
            get
            {
                return (string) base["endpointConfiguration"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["endpointConfiguration"] = value;
            }
        }

        [ConfigurationProperty("headers")]
        public AddressHeaderCollectionElement Headers
        {
            get
            {
                return (AddressHeaderCollectionElement) base["headers"];
            }
        }

        [ConfigurationProperty("identity")]
        public IdentityElement Identity
        {
            get
            {
                return (IdentityElement) base["identity"];
            }
        }

        [ConfigurationProperty("isSystemEndpoint", DefaultValue=false)]
        public bool IsSystemEndpoint
        {
            get
            {
                return (bool) base["isSystemEndpoint"];
            }
            set
            {
                base["isSystemEndpoint"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("kind", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey)]
        public string Kind
        {
            get
            {
                return (string) base["kind"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["kind"] = value;
            }
        }

        [ConfigurationProperty("listenUri", DefaultValue=null)]
        public Uri ListenUri
        {
            get
            {
                return (Uri) base["listenUri"];
            }
            set
            {
                base["listenUri"] = value;
            }
        }

        [ConfigurationProperty("listenUriMode", DefaultValue=0), ServiceModelEnumValidator(typeof(ListenUriModeHelper))]
        public System.ServiceModel.Description.ListenUriMode ListenUriMode
        {
            get
            {
                return (System.ServiceModel.Description.ListenUriMode) base["listenUriMode"];
            }
            set
            {
                base["listenUriMode"] = value;
            }
        }

        [ConfigurationProperty("name", DefaultValue=""), StringValidator(MinLength=0)]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = string.Empty;
                }
                base["name"] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("address", typeof(Uri), "", null, null, ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("behaviorConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("binding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bindingName", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("bindingNamespace", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("contract", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("headers", typeof(AddressHeaderCollectionElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("identity", typeof(IdentityElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("listenUriMode", typeof(System.ServiceModel.Description.ListenUriMode), System.ServiceModel.Description.ListenUriMode.Explicit, null, new ServiceModelEnumValidator(typeof(ListenUriModeHelper)), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("listenUri", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("isSystemEndpoint", typeof(bool), false, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("kind", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("endpointConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

