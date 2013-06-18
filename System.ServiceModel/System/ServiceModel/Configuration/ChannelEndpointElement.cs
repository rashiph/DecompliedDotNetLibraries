namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;

    public sealed class ChannelEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private ConfigurationPropertyCollection properties;

        public ChannelEndpointElement()
        {
        }

        public ChannelEndpointElement(EndpointAddress address, string contractType) : this()
        {
            if (address != null)
            {
                this.Address = address.Uri;
                this.Headers.Headers = address.Headers;
                if (address.Identity != null)
                {
                    this.Identity.InitializeFrom(address.Identity);
                }
            }
            this.Contract = contractType;
        }

        internal void Copy(ChannelEndpointElement source)
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
            if (properties["kind"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Kind = source.Kind;
            }
            if (properties["endpointConfiguration"].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.EndpointConfiguration = source.EndpointConfiguration;
            }
        }

        [SecurityCritical]
        protected override void Reset(ConfigurationElement parentElement)
        {
            this.contextHelper.OnReset(parentElement);
            base.Reset(parentElement);
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        [SecurityCritical]
        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return this.contextHelper.GetOriginalContext(this);
        }

        [ConfigurationProperty("address", Options=ConfigurationPropertyOptions.None)]
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

        [ConfigurationProperty("binding", DefaultValue=""), StringValidator(MinLength=0)]
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

        [ConfigurationProperty("bindingConfiguration", DefaultValue=""), StringValidator(MinLength=0)]
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

        [ConfigurationProperty("endpointConfiguration", DefaultValue="", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
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

        [ConfigurationProperty("kind", DefaultValue="", Options=ConfigurationPropertyOptions.None), StringValidator(MinLength=0)]
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

        [ConfigurationProperty("name", DefaultValue="", Options=ConfigurationPropertyOptions.IsKey), StringValidator(MinLength=0)]
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
                    propertys.Add(new ConfigurationProperty("address", typeof(Uri), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("behaviorConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("binding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("contract", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("headers", typeof(AddressHeaderCollectionElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("identity", typeof(IdentityElement), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    propertys.Add(new ConfigurationProperty("kind", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("endpointConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

