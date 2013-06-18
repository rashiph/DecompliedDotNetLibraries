namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    public sealed class IssuedTokenParametersEndpointAddressElement : EndpointAddressElementBase, IConfigurationContextProviderInternal
    {
        private ConfigurationPropertyCollection properties;

        internal void Copy(IssuedTokenParametersEndpointAddressElement source)
        {
            base.Copy(source);
            this.BindingConfiguration = source.BindingConfiguration;
            this.Binding = source.Binding;
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return null;
        }

        internal void Validate()
        {
            ContextInformation evaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            if ((evaluationContext != null) && !string.IsNullOrEmpty(this.Binding))
            {
                BindingsSection.ValidateBindingReference(this.Binding, this.BindingConfiguration, evaluationContext, this);
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

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection properties = base.Properties;
                    properties.Add(new ConfigurationProperty("binding", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    properties.Add(new ConfigurationProperty("bindingConfiguration", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = properties;
                }
                return this.properties;
            }
        }
    }
}

