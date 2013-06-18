namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public sealed class StandardEndpointsSection : ConfigurationSection, IConfigurationContextProviderInternal
    {
        private static System.Configuration.Configuration configuration;
        private ConfigurationPropertyCollection properties;

        public static StandardEndpointsSection GetSection(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("config");
            }
            return (StandardEndpointsSection) config.GetSection(ConfigurationStrings.StandardEndpointsSectionPath);
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointExtensionNotFound", new object[] { ConfigurationHelpers.GetEndpointsSectionPath(elementName) })));
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return null;
        }

        internal static bool TryAdd(string name, ServiceEndpoint endpoint, out string endpointSectionName)
        {
            if (Configuration == null)
            {
                DiagnosticUtility.FailFast("The TryAdd(string name, ServiceEndpoint endpoint, Configuration config, out string endpointSectionName) variant of this function should always be called first. The Configuration object is not set.");
            }
            bool flag = false;
            string str = null;
            StandardEndpointsSection section = GetSection(Configuration);
            section.UpdateEndpointSections();
            foreach (string str2 in section.EndpointCollectionElements.Keys)
            {
                EndpointCollectionElement element = section.EndpointCollectionElements[str2];
                MethodInfo method = element.GetType().GetMethod("TryAdd", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    flag = (bool) method.Invoke(element, new object[] { name, endpoint, Configuration });
                    if (flag)
                    {
                        str = str2;
                        break;
                    }
                }
            }
            endpointSectionName = str;
            return flag;
        }

        internal static bool TryAdd(string name, ServiceEndpoint endpoint, System.Configuration.Configuration config, out string endpointSectionName)
        {
            bool flag = false;
            Configuration = config;
            try
            {
                flag = TryAdd(name, endpoint, out endpointSectionName);
            }
            finally
            {
                Configuration = null;
            }
            return flag;
        }

        private void UpdateEndpointSections()
        {
            this.UpdateEndpointSections(ConfigurationHelpers.GetEvaluationContext(this));
        }

        [SecuritySafeCritical]
        internal void UpdateEndpointSections(ContextInformation evaluationContext)
        {
            ExtensionElementCollection elements = ExtensionsSection.UnsafeLookupCollection("endpointExtensions", evaluationContext);
            if (elements.Count != this.properties.Count)
            {
                foreach (ExtensionElement element in elements)
                {
                    if ((element != null) && !this.properties.Contains(element.Name))
                    {
                        Type type = Type.GetType(element.Type, false);
                        if (type == null)
                        {
                            ConfigurationHelpers.TraceExtensionTypeNotFound(element);
                        }
                        else
                        {
                            ConfigurationProperty property = new ConfigurationProperty(element.Name, type, null, ConfigurationPropertyOptions.None);
                            this.properties.Add(property);
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal static void ValidateEndpointReference(string endpoint, string endpointConfiguration, ContextInformation evaluationContext, ConfigurationElement configurationElement)
        {
            if (evaluationContext == null)
            {
                DiagnosticUtility.FailFast("ValidateEndpointReference() should only called with valid ContextInformation");
            }
            if (!string.IsNullOrEmpty(endpoint))
            {
                EndpointCollectionElement element = null;
                if (evaluationContext != null)
                {
                    element = ConfigurationHelpers.UnsafeGetAssociatedEndpointCollectionElement(evaluationContext, endpoint);
                }
                else
                {
                    element = ConfigurationHelpers.UnsafeGetEndpointCollectionElement(endpoint);
                }
                if (element == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidSection", new object[] { ConfigurationHelpers.GetEndpointsSectionPath(endpoint) }), configurationElement.ElementInformation.Source, configurationElement.ElementInformation.LineNumber));
                }
                if (!string.IsNullOrEmpty(endpointConfiguration) && !element.ContainsKey(endpointConfiguration))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidEndpointName", new object[] { endpointConfiguration, ConfigurationHelpers.GetEndpointsSectionPath(endpoint), "endpointConfiguration" }), configurationElement.ElementInformation.Source, configurationElement.ElementInformation.LineNumber));
                }
            }
        }

        private static System.Configuration.Configuration Configuration
        {
            get
            {
                return configuration;
            }
            set
            {
                configuration = value;
            }
        }

        private Dictionary<string, EndpointCollectionElement> EndpointCollectionElements
        {
            get
            {
                Dictionary<string, EndpointCollectionElement> dictionary = new Dictionary<string, EndpointCollectionElement>();
                foreach (ConfigurationProperty property in this.Properties)
                {
                    dictionary.Add(property.Name, this[property.Name]);
                }
                return dictionary;
            }
        }

        public List<EndpointCollectionElement> EndpointCollections
        {
            get
            {
                List<EndpointCollectionElement> list = new List<EndpointCollectionElement>();
                foreach (ConfigurationProperty property in this.Properties)
                {
                    list.Add(this[property.Name]);
                }
                return list;
            }
        }

        public EndpointCollectionElement this[string endpoint]
        {
            get
            {
                return (EndpointCollectionElement) base[endpoint];
            }
        }

        [ConfigurationProperty("mexEndpoint", Options=ConfigurationPropertyOptions.None)]
        public ServiceMetadataEndpointCollectionElement MexEndpoint
        {
            get
            {
                return (ServiceMetadataEndpointCollectionElement) base["mexEndpoint"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection();
                }
                this.UpdateEndpointSections();
                return this.properties;
            }
        }
    }
}

