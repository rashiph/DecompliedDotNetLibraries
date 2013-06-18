namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;

    public class BehaviorsSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        internal static BehaviorsSection GetSection()
        {
            return (BehaviorsSection) ConfigurationHelpers.GetSection(ConfigurationStrings.BehaviorsSectionPath);
        }

        [SecurityCritical]
        internal static BehaviorsSection UnsafeGetAssociatedSection(ContextInformation evalContext)
        {
            return (BehaviorsSection) ConfigurationHelpers.UnsafeGetAssociatedSection(evalContext, ConfigurationStrings.BehaviorsSectionPath);
        }

        [SecurityCritical]
        internal static BehaviorsSection UnsafeGetSection()
        {
            return (BehaviorsSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.BehaviorsSectionPath);
        }

        [SecuritySafeCritical]
        internal static void ValidateEndpointBehaviorReference(string behaviorConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            if (evaluationContext == null)
            {
                DiagnosticUtility.FailFast("ValidateBehaviorReference() should only called with valid ContextInformation");
            }
            if (!string.IsNullOrEmpty(behaviorConfiguration))
            {
                BehaviorsSection section = (BehaviorsSection) ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.BehaviorsSectionPath);
                if (!section.EndpointBehaviors.ContainsKey(behaviorConfiguration))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidEndpointBehavior", new object[] { behaviorConfiguration }), configurationElement.ElementInformation.Source, configurationElement.ElementInformation.LineNumber));
                }
            }
        }

        [SecuritySafeCritical]
        internal static void ValidateServiceBehaviorReference(string behaviorConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            if (evaluationContext == null)
            {
                DiagnosticUtility.FailFast("ValidateBehaviorReference() should only called with valid ContextInformation");
            }
            if (!string.IsNullOrEmpty(behaviorConfiguration))
            {
                BehaviorsSection section = (BehaviorsSection) ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.BehaviorsSectionPath);
                if (!section.ServiceBehaviors.ContainsKey(behaviorConfiguration))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidServiceBehavior", new object[] { behaviorConfiguration }), configurationElement.ElementInformation.Source, configurationElement.ElementInformation.LineNumber));
                }
            }
        }

        [ConfigurationProperty("endpointBehaviors", Options=ConfigurationPropertyOptions.None)]
        public EndpointBehaviorElementCollection EndpointBehaviors
        {
            get
            {
                return (EndpointBehaviorElementCollection) base["endpointBehaviors"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("endpointBehaviors", typeof(EndpointBehaviorElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("serviceBehaviors", typeof(ServiceBehaviorElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ConfigurationProperty("serviceBehaviors", Options=ConfigurationPropertyOptions.None)]
        public ServiceBehaviorElementCollection ServiceBehaviors
        {
            get
            {
                return (ServiceBehaviorElementCollection) base["serviceBehaviors"];
            }
        }
    }
}

