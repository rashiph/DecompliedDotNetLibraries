namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public abstract class StandardEndpointElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private ConfigurationPropertyCollection properties;

        protected StandardEndpointElement()
        {
        }

        public void ApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (channelEndpointElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelEndpointElement");
            }
            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForEndpoint", new object[] { this.EndpointType.AssemblyQualifiedName, endpoint.GetType().AssemblyQualifiedName }));
            }
            this.OnApplyConfiguration(endpoint, channelEndpointElement);
        }

        public void ApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (serviceEndpointElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpointElement");
            }
            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForEndpoint", new object[] { (this.EndpointType == null) ? string.Empty : this.EndpointType.AssemblyQualifiedName, endpoint.GetType().AssemblyQualifiedName }));
            }
            this.OnApplyConfiguration(endpoint, serviceEndpointElement);
        }

        protected internal abstract ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription);
        public void InitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
            if (channelEndpointElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelEndpointElement");
            }
            this.OnInitializeAndValidate(channelEndpointElement);
        }

        public void InitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (serviceEndpointElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpointElement");
            }
            this.OnInitializeAndValidate(serviceEndpointElement);
        }

        protected internal virtual void InitializeFrom(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (endpoint.GetType() != this.EndpointType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("ConfigInvalidTypeForEndpoint", new object[] { (this.EndpointType == null) ? string.Empty : this.EndpointType.AssemblyQualifiedName, endpoint.GetType().AssemblyQualifiedName }));
            }
        }

        protected abstract void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement);
        protected abstract void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement);
        protected abstract void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement);
        protected abstract void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement);
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

        protected internal abstract Type EndpointType { get; }

        [StringValidator(MinLength=0), ConfigurationProperty("name", Options=ConfigurationPropertyOptions.IsKey)]
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
                    propertys.Add(new ConfigurationProperty("name", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.IsKey));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

