namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public abstract class EndpointCollectionElement : ConfigurationElement, IConfigurationContextProviderInternal
    {
        private string endpointName = string.Empty;

        protected EndpointCollectionElement()
        {
        }

        public abstract bool ContainsKey(string name);
        protected internal abstract StandardEndpointElement GetDefaultStandardEndpointElement();
        [SecuritySafeCritical]
        private string GetEndpointName()
        {
            string name = string.Empty;
            ExtensionElementCollection elements = null;
            Type type = base.GetType();
            elements = ExtensionsSection.UnsafeLookupCollection("endpointExtensions", ConfigurationHelpers.GetEvaluationContext(this));
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigExtensionCollectionNotFound", new object[] { "endpointExtensions" }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            for (int i = 0; i < elements.Count; i++)
            {
                ExtensionElement element = elements[i];
                if (element.Type.Equals(type.AssemblyQualifiedName, StringComparison.Ordinal))
                {
                    name = element.Name;
                    break;
                }
                Type o = Type.GetType(element.Type, false);
                if ((null != o) && type.Equals(o))
                {
                    name = element.Name;
                    break;
                }
            }
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigExtensionTypeNotRegisteredInCollection", new object[] { type.AssemblyQualifiedName, "endpointExtensions" }), base.ElementInformation.Source, base.ElementInformation.LineNumber));
            }
            return name;
        }

        ContextInformation IConfigurationContextProviderInternal.GetEvaluationContext()
        {
            return base.EvaluationContext;
        }

        ContextInformation IConfigurationContextProviderInternal.GetOriginalEvaluationContext()
        {
            return null;
        }

        protected internal abstract bool TryAdd(string name, ServiceEndpoint endpoint, System.Configuration.Configuration config);

        public abstract ReadOnlyCollection<StandardEndpointElement> ConfiguredEndpoints { get; }

        public string EndpointName
        {
            get
            {
                if (string.IsNullOrEmpty(this.endpointName))
                {
                    this.endpointName = this.GetEndpointName();
                }
                return this.endpointName;
            }
        }

        public abstract Type EndpointType { get; }
    }
}

