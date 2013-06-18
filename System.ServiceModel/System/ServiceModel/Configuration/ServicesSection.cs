namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;

    public sealed class ServicesSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        [SecurityCritical]
        private EvaluationContextHelper contextHelper;
        private ConfigurationPropertyCollection properties;

        internal static ServicesSection GetSection()
        {
            return (ServicesSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ServicesSectionPath);
        }

        protected override void PostDeserialize()
        {
            this.ValidateSection();
            base.PostDeserialize();
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

        [SecurityCritical]
        internal static ServicesSection UnsafeGetSection()
        {
            return (ServicesSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ServicesSectionPath);
        }

        private void ValidateSection()
        {
            ContextInformation evaluationContext = ConfigurationHelpers.GetEvaluationContext(this);
            if (evaluationContext != null)
            {
                foreach (ServiceElement element in this.Services)
                {
                    BehaviorsSection.ValidateServiceBehaviorReference(element.BehaviorConfiguration, evaluationContext, element);
                    foreach (ServiceEndpointElement element2 in element.Endpoints)
                    {
                        if (string.IsNullOrEmpty(element2.Kind))
                        {
                            if (!string.IsNullOrEmpty(element2.EndpointConfiguration))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidAttribute", new object[] { "endpointConfiguration", "endpoint", "kind" })));
                            }
                            if (string.IsNullOrEmpty(element2.Binding))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("RequiredAttributeMissing", new object[] { "binding", "endpoint" })));
                            }
                        }
                        if (string.IsNullOrEmpty(element2.Binding) && !string.IsNullOrEmpty(element2.BindingConfiguration))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidAttribute", new object[] { "bindingConfiguration", "endpoint", "binding" })));
                        }
                        BehaviorsSection.ValidateEndpointBehaviorReference(element2.BehaviorConfiguration, evaluationContext, element2);
                        BindingsSection.ValidateBindingReference(element2.Binding, element2.BindingConfiguration, evaluationContext, element2);
                        StandardEndpointsSection.ValidateEndpointReference(element2.Kind, element2.EndpointConfiguration, evaluationContext, element2);
                    }
                }
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("", typeof(ServiceElementCollection), null, null, null, ConfigurationPropertyOptions.IsDefaultCollection));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("", Options=ConfigurationPropertyOptions.IsDefaultCollection)]
        public ServiceElementCollection Services
        {
            get
            {
                return (ServiceElementCollection) base[""];
            }
        }
    }
}

