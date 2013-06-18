namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;

    public sealed class ClientSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        private ConfigurationPropertyCollection properties;

        internal static ClientSection GetSection()
        {
            return (ClientSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ClientSectionPath);
        }

        protected override void InitializeDefault()
        {
            this.Metadata.SetDefaults();
        }

        protected override void PostDeserialize()
        {
            this.ValidateSection();
            base.PostDeserialize();
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return null;
        }

        [SecurityCritical]
        internal static ClientSection UnsafeGetSection()
        {
            return (ClientSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ClientSectionPath);
        }

        [SecurityCritical]
        internal static ClientSection UnsafeGetSection(ContextInformation contextInformation)
        {
            return (ClientSection) ConfigurationHelpers.UnsafeGetSectionFromContext(contextInformation, ConfigurationStrings.ClientSectionPath);
        }

        private void ValidateSection()
        {
            ContextInformation evaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            if (evaluationContext != null)
            {
                foreach (ChannelEndpointElement element in this.Endpoints)
                {
                    if (string.IsNullOrEmpty(element.Kind))
                    {
                        if (!string.IsNullOrEmpty(element.EndpointConfiguration))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidAttribute", new object[] { "endpointConfiguration", "endpoint", "kind" })));
                        }
                        if (string.IsNullOrEmpty(element.Binding))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { "binding", "endpoint" })));
                        }
                        if (string.IsNullOrEmpty(element.Contract))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { "contract", "endpoint" })));
                        }
                    }
                    if (string.IsNullOrEmpty(element.Binding) && !string.IsNullOrEmpty(element.BindingConfiguration))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidAttribute", new object[] { "bindingConfiguration", "endpoint", "binding" })));
                    }
                    BehaviorsSection.ValidateEndpointBehaviorReference(element.BehaviorConfiguration, evaluationContext, element);
                    BindingsSection.ValidateBindingReference(element.Binding, element.BindingConfiguration, evaluationContext, element);
                    StandardEndpointsSection.ValidateEndpointReference(element.Kind, element.EndpointConfiguration, evaluationContext, element);
                }
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ChannelEndpointElementCollection Endpoints
        {
            get
            {
                return (ChannelEndpointElementCollection) base[""];
            }
        }

        [ConfigurationProperty("metadata")]
        public MetadataElement Metadata
        {
            get
            {
                return (MetadataElement) base["metadata"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(ChannelEndpointElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    propertys.Add(new ConfigurationProperty("metadata", typeof(MetadataElement), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

