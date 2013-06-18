namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;

    public class WorkflowControlEndpointElement : StandardEndpointElement
    {
        private ConfigurationPropertyCollection properties;

        protected internal override ServiceEndpoint CreateServiceEndpoint(ContractDescription contractDescription)
        {
            WorkflowControlEndpoint endpoint = new WorkflowControlEndpoint();
            if (!string.IsNullOrEmpty(this.Binding))
            {
                System.ServiceModel.Channels.Binding binding = ConfigLoader.LookupBinding(this.Binding, this.BindingConfiguration);
                if (binding == null)
                {
                    throw FxTrace.Exception.AsError(new ConfigurationErrorsException(System.ServiceModel.Activities.SR.FailedToLoadBindingInControlEndpoint(this.Binding, this.BindingConfiguration, base.Name)));
                }
                endpoint.Binding = binding;
            }
            return endpoint;
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ChannelEndpointElement channelEndpointElement)
        {
        }

        protected override void OnApplyConfiguration(ServiceEndpoint endpoint, ServiceEndpointElement serviceEndpointElement)
        {
        }

        protected override void OnInitializeAndValidate(ChannelEndpointElement channelEndpointElement)
        {
        }

        protected override void OnInitializeAndValidate(ServiceEndpointElement serviceEndpointElement)
        {
            if (serviceEndpointElement.Address == null)
            {
                serviceEndpointElement.Address = this.Address;
            }
        }

        [ConfigurationProperty("address", DefaultValue="")]
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

        [StringValidator(MinLength=0), ConfigurationProperty("bindingConfiguration", DefaultValue="")]
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

        protected internal override System.Type EndpointType
        {
            get
            {
                return typeof(WorkflowControlEndpoint);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("binding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("address", typeof(Uri), string.Empty, null, null, ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

