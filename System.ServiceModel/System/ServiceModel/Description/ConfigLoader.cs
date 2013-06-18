namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.Net;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.Text;

    internal class ConfigLoader
    {
        private Dictionary<string, Binding> bindingTable;
        private ContextInformation configurationContext;
        [SecurityCritical]
        private static System.Configuration.ConfigurationPermission configurationPermission;
        private IContractResolver contractResolver;
        private static readonly object[] emptyObjectArray = new object[0];
        private static readonly System.Type[] emptyTypeArray = new System.Type[0];
        [ThreadStatic]
        private static List<string> resolvedBindings;
        [ThreadStatic]
        private static List<string> resolvedEndpoints;

        public ConfigLoader() : this((IContractResolver) null)
        {
        }

        public ConfigLoader(ContextInformation configurationContext) : this((IContractResolver) null)
        {
            this.configurationContext = configurationContext;
        }

        public ConfigLoader(IContractResolver contractResolver)
        {
            this.contractResolver = contractResolver;
            this.bindingTable = new Dictionary<string, Binding>();
        }

        private static void CheckAccess(IConfigurationContextProviderInternal element)
        {
            if (IsConfigAboveApplication(ConfigurationHelpers.GetOriginalEvaluationContext(element)))
            {
                ConfigurationPermission.Demand();
            }
        }

        [SecuritySafeCritical]
        private static void ConfigureEndpoint(StandardEndpointElement standardEndpointElement, ChannelEndpointElement channelEndpointElement, EndpointAddress address, ContextInformation context, ContractDescription contract, out ServiceEndpoint endpoint)
        {
            ChannelEndpointElement element = new ChannelEndpointElement();
            element.Copy(channelEndpointElement);
            standardEndpointElement.InitializeAndValidate(element);
            endpoint = standardEndpointElement.CreateServiceEndpoint(contract);
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigNoEndpointCreated", new object[] { standardEndpointElement.GetType().AssemblyQualifiedName, (standardEndpointElement.EndpointType == null) ? string.Empty : standardEndpointElement.EndpointType.AssemblyQualifiedName })));
            }
            if (!string.IsNullOrEmpty(element.Binding))
            {
                endpoint.Binding = LookupBinding(element.Binding, element.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            }
            if (!string.IsNullOrEmpty(element.Name))
            {
                endpoint.Name = element.Name;
            }
            if (address != null)
            {
                endpoint.Address = address;
            }
            if (((endpoint.Address == null) && (element.Address != null)) && (element.Address.OriginalString.Length > 0))
            {
                endpoint.Address = new EndpointAddress(element.Address, LoadIdentity(element.Identity), element.Headers.Headers);
            }
            CommonBehaviorsSection commonBehaviors = LookupCommonBehaviors(ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            if ((commonBehaviors != null) && (commonBehaviors.EndpointBehaviors != null))
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, endpoint.Behaviors, true);
            }
            EndpointBehaviorElement endpointBehaviors = LookupEndpointBehaviors(element.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(channelEndpointElement));
            if (endpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(endpointBehaviors, endpoint.Behaviors, false);
            }
            standardEndpointElement.ApplyConfiguration(endpoint, element);
        }

        [SecuritySafeCritical]
        private void ConfigureEndpoint(StandardEndpointElement standardEndpointElement, ServiceEndpointElement serviceEndpointElement, ContextInformation context, ServiceHostBase host, System.ServiceModel.Description.ServiceDescription description, out ServiceEndpoint endpoint, bool omitSettingEndpointAddress = false)
        {
            ServiceEndpointElement element = new ServiceEndpointElement();
            element.Copy(serviceEndpointElement);
            standardEndpointElement.InitializeAndValidate(element);
            ContractDescription contractDescription = null;
            if (!string.IsNullOrEmpty(element.Contract))
            {
                contractDescription = this.LookupContractForStandardEndpoint(element.Contract, description.Name);
            }
            endpoint = standardEndpointElement.CreateServiceEndpoint(contractDescription);
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ConfigNoEndpointCreated", new object[] { standardEndpointElement.GetType().AssemblyQualifiedName, (standardEndpointElement.EndpointType == null) ? string.Empty : standardEndpointElement.EndpointType.AssemblyQualifiedName })));
            }
            Binding binding = null;
            if (!string.IsNullOrEmpty(element.Binding))
            {
                string key = element.Binding + ":" + element.BindingConfiguration;
                if (!this.bindingTable.TryGetValue(key, out binding))
                {
                    binding = LookupBinding(element.Binding, element.BindingConfiguration, context);
                    this.bindingTable.Add(key, binding);
                }
            }
            else
            {
                binding = endpoint.Binding;
            }
            if (binding != null)
            {
                if (!string.IsNullOrEmpty(element.BindingName))
                {
                    binding.Name = element.BindingName;
                }
                if (!string.IsNullOrEmpty(element.BindingNamespace))
                {
                    binding.Namespace = element.BindingNamespace;
                }
                endpoint.Binding = binding;
                if (!omitSettingEndpointAddress)
                {
                    ConfigureEndpointAddress(element, host, endpoint);
                    ConfigureEndpointListenUri(element, host, endpoint);
                }
            }
            endpoint.ListenUriMode = element.ListenUriMode;
            if (!string.IsNullOrEmpty(element.Name))
            {
                endpoint.Name = element.Name;
            }
            KeyedByTypeCollection<IEndpointBehavior> behaviors = endpoint.Behaviors;
            EndpointBehaviorElement endpointBehaviors = LookupEndpointBehaviors(element.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(serviceEndpointElement));
            if (endpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(endpointBehaviors, behaviors, false);
            }
            if (element.ElementInformation.Properties["isSystemEndpoint"].ValueOrigin != PropertyValueOrigin.Default)
            {
                endpoint.IsSystemEndpoint = element.IsSystemEndpoint;
            }
            standardEndpointElement.ApplyConfiguration(endpoint, element);
        }

        internal static void ConfigureEndpointAddress(ServiceEndpointElement serviceEndpointElement, ServiceHostBase host, ServiceEndpoint endpoint)
        {
            if (serviceEndpointElement.Address != null)
            {
                Uri uri = ServiceHostBase.MakeAbsoluteUri(serviceEndpointElement.Address, endpoint.Binding, host.InternalBaseAddresses);
                endpoint.Address = new EndpointAddress(uri, LoadIdentity(serviceEndpointElement.Identity), serviceEndpointElement.Headers.Headers);
                endpoint.UnresolvedAddress = serviceEndpointElement.Address;
            }
        }

        internal static void ConfigureEndpointListenUri(ServiceEndpointElement serviceEndpointElement, ServiceHostBase host, ServiceEndpoint endpoint)
        {
            if (serviceEndpointElement.ListenUri != null)
            {
                endpoint.ListenUri = ServiceHostBase.MakeAbsoluteUri(serviceEndpointElement.ListenUri, endpoint.Binding, host.InternalBaseAddresses);
                endpoint.UnresolvedListenUri = serviceEndpointElement.ListenUri;
            }
        }

        [SecurityCritical]
        private static BindingCollectionElement GetBindingCollectionElement(string bindingSectionName, ContextInformation context)
        {
            if (string.IsNullOrEmpty(bindingSectionName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingTypeCannotBeNullOrEmpty")));
            }
            if (context == null)
            {
                return ConfigurationHelpers.UnsafeGetBindingCollectionElement(bindingSectionName);
            }
            return ConfigurationHelpers.UnsafeGetAssociatedBindingCollectionElement(context, bindingSectionName);
        }

        private static bool IsChannelElementMatch(ChannelEndpointElement channelElement, ContractDescription contract, EndpointAddress address, bool useChannelElementKind, out ServiceEndpoint serviceEndpoint)
        {
            serviceEndpoint = null;
            if (string.IsNullOrEmpty(channelElement.Kind))
            {
                return (channelElement.Contract == contract.ConfigurationName);
            }
            if (useChannelElementKind)
            {
                serviceEndpoint = LookupEndpoint(channelElement, null, address, contract);
                if (serviceEndpoint == null)
                {
                    return false;
                }
                if ((serviceEndpoint.Contract.ConfigurationName == contract.ConfigurationName) && (string.IsNullOrEmpty(channelElement.Contract) || (contract.ConfigurationName == channelElement.Contract)))
                {
                    return true;
                }
                serviceEndpoint = null;
            }
            return false;
        }

        private static bool IsConfigAboveApplication(ContextInformation contextInformation)
        {
            if (contextInformation == null)
            {
                return true;
            }
            if (contextInformation.IsMachineLevel)
            {
                return true;
            }
            if (contextInformation.HostingContext is ExeContext)
            {
                return false;
            }
            return IsWebConfigAboveApplication(contextInformation);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsWebConfigAboveApplication(ContextInformation contextInformation)
        {
            return AspNetEnvironment.Current.IsWebConfigAboveApplication(contextInformation.HostingContext);
        }

        private static bool IsWildcardMatch(string endpointConfigurationName)
        {
            return string.Equals(endpointConfigurationName, "*", StringComparison.Ordinal);
        }

        [SecuritySafeCritical]
        private static void LoadBehaviors<T>(ServiceModelExtensionCollectionElement<BehaviorExtensionElement> behaviorElement, KeyedByTypeCollection<T> behaviors, bool commonBehaviors)
        {
            bool? isPT = null;
            KeyedByTypeCollection<T> types = new KeyedByTypeCollection<T>();
            for (int i = 0; i < behaviorElement.Count; i++)
            {
                BehaviorExtensionElement behaviorExtension = behaviorElement[i];
                object obj2 = behaviorExtension.CreateBehavior();
                if (obj2 != null)
                {
                    System.Type c = obj2.GetType();
                    if (!typeof(T).IsAssignableFrom(c))
                    {
                        TraceBehaviorWarning(behaviorExtension, 0x80035, System.ServiceModel.SR.GetString("TraceCodeSkipBehavior"), c, typeof(T));
                    }
                    else if (commonBehaviors && ShouldSkipCommonBehavior(c, ref isPT))
                    {
                        TraceBehaviorWarning(behaviorExtension, 0x80035, System.ServiceModel.SR.GetString("TraceCodeSkipBehavior"), c, typeof(T));
                    }
                    else
                    {
                        types.Add((T) obj2);
                        if (behaviors.Contains(c))
                        {
                            TraceBehaviorWarning(behaviorExtension, 0x8002a, System.ServiceModel.SR.GetString("TraceCodeRemoveBehavior"), c, typeof(T));
                            behaviors.Remove(c);
                        }
                        behaviors.Add((T) obj2);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private static void LoadChannelBehaviors(EndpointBehaviorElement behaviorElement, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
        {
            if (behaviorElement != null)
            {
                LoadBehaviors<IEndpointBehavior>(behaviorElement, channelBehaviors, false);
            }
        }

        [SecuritySafeCritical]
        internal void LoadChannelBehaviors(ServiceEndpoint serviceEndpoint, string configurationName)
        {
            ServiceEndpoint endpoint;
            bool wildcard = IsWildcardMatch(configurationName);
            ChannelEndpointElement provider = LookupChannel(this.configurationContext, configurationName, serviceEndpoint.Contract, null, wildcard, false, out endpoint);
            if (provider == null)
            {
                if (wildcard)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConfigContractNotFound", new object[] { serviceEndpoint.Contract.ConfigurationName })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConfigChannelConfigurationNotFound", new object[] { configurationName, serviceEndpoint.Contract.ConfigurationName })));
            }
            if ((serviceEndpoint.Binding == null) && !string.IsNullOrEmpty(provider.Binding))
            {
                serviceEndpoint.Binding = LookupBinding(provider.Binding, provider.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(provider));
            }
            if (((serviceEndpoint.Address == null) && (provider.Address != null)) && (provider.Address.OriginalString.Length > 0))
            {
                serviceEndpoint.Address = new EndpointAddress(provider.Address, LoadIdentity(provider.Identity), provider.Headers.Headers);
            }
            CommonBehaviorsSection commonBehaviors = LookupCommonBehaviors(ConfigurationHelpers.GetEvaluationContext(provider));
            if ((commonBehaviors != null) && (commonBehaviors.EndpointBehaviors != null))
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, serviceEndpoint.Behaviors, true);
            }
            EndpointBehaviorElement endpointBehaviors = LookupEndpointBehaviors(provider.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(provider));
            if (endpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(endpointBehaviors, serviceEndpoint.Behaviors, false);
            }
        }

        [SecuritySafeCritical]
        internal static void LoadChannelBehaviors(string behaviorName, ContextInformation context, KeyedByTypeCollection<IEndpointBehavior> channelBehaviors)
        {
            LoadChannelBehaviors(LookupEndpointBehaviors(behaviorName, context), channelBehaviors);
        }

        [SecuritySafeCritical]
        internal void LoadCommonClientBehaviors(ServiceEndpoint serviceEndpoint)
        {
            CommonBehaviorsSection commonBehaviors = LookupCommonBehaviors(this.configurationContext);
            if ((commonBehaviors != null) && (commonBehaviors.EndpointBehaviors != null))
            {
                LoadBehaviors<IEndpointBehavior>(commonBehaviors.EndpointBehaviors, serviceEndpoint.Behaviors, true);
            }
        }

        [SecuritySafeCritical]
        public static void LoadDefaultEndpointBehaviors(ServiceEndpoint endpoint)
        {
            EndpointBehaviorElement endpointBehaviors = LookupEndpointBehaviors("", ConfigurationHelpers.GetEvaluationContext(null));
            if (endpointBehaviors != null)
            {
                LoadBehaviors<IEndpointBehavior>(endpointBehaviors, endpoint.Behaviors, false);
            }
        }

        [SecuritySafeCritical]
        internal static EndpointAddress LoadEndpointAddress(EndpointAddressElementBase element)
        {
            return new EndpointAddress(element.Address, LoadIdentity(element.Identity), element.Headers.Headers);
        }

        [SecuritySafeCritical]
        private void LoadHostConfig(ServiceElement serviceElement, ServiceHostBase host, Action<Uri> addBaseAddress)
        {
            HostElement element = serviceElement.Host;
            if (element != null)
            {
                if (!AspNetEnvironment.Enabled)
                {
                    foreach (BaseAddressElement element2 in element.BaseAddresses)
                    {
                        Uri uri;
                        string uriString = null;
                        string baseAddress = element2.BaseAddress;
                        int index = baseAddress.IndexOf(':');
                        if ((((index != -1) && (baseAddress.Length >= (index + 4))) && ((baseAddress[index + 1] == '/') && (baseAddress[index + 2] == '/'))) && (baseAddress[index + 3] == '*'))
                        {
                            string str3 = baseAddress.Substring(0, index + 3);
                            string str4 = baseAddress.Substring(index + 4);
                            StringBuilder builder = new StringBuilder(str3);
                            builder.Append(Dns.GetHostName());
                            builder.Append(str4);
                            uriString = builder.ToString();
                        }
                        if (uriString == null)
                        {
                            uriString = baseAddress;
                        }
                        if (!Uri.TryCreate(uriString, UriKind.Absolute, out uri))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("BaseAddressMustBeAbsolute")));
                        }
                        addBaseAddress(uri);
                    }
                }
                HostTimeoutsElement timeouts = element.Timeouts;
                if (timeouts != null)
                {
                    if (timeouts.OpenTimeout != TimeSpan.Zero)
                    {
                        host.OpenTimeout = timeouts.OpenTimeout;
                    }
                    if (timeouts.CloseTimeout != TimeSpan.Zero)
                    {
                        host.CloseTimeout = timeouts.CloseTimeout;
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal static EndpointIdentity LoadIdentity(IdentityElement element)
        {
            EndpointIdentity identity = null;
            PropertyInformationCollection properties = element.ElementInformation.Properties;
            if (properties["userPrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateUpnIdentity(element.UserPrincipalName.Value);
            }
            if (properties["servicePrincipalName"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateSpnIdentity(element.ServicePrincipalName.Value);
            }
            if (properties["dns"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateDnsIdentity(element.Dns.Value);
            }
            if (properties["rsa"].ValueOrigin != PropertyValueOrigin.Default)
            {
                return EndpointIdentity.CreateRsaIdentity(element.Rsa.Value);
            }
            if (properties["certificate"].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509Certificate2Collection supportingCertificates = new X509Certificate2Collection();
                supportingCertificates.Import(Convert.FromBase64String(element.Certificate.EncodedValue));
                if (supportingCertificates.Count == 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnableToLoadCertificateIdentity")));
                }
                X509Certificate2 primaryCertificate = supportingCertificates[0];
                supportingCertificates.RemoveAt(0);
                return EndpointIdentity.CreateX509CertificateIdentity(primaryCertificate, supportingCertificates);
            }
            if (properties["certificateReference"].ValueOrigin != PropertyValueOrigin.Default)
            {
                X509CertificateStore store = new X509CertificateStore(element.CertificateReference.StoreName, element.CertificateReference.StoreLocation);
                X509Certificate2Collection certificates = null;
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    certificates = store.Find(element.CertificateReference.X509FindType, element.CertificateReference.FindValue, false);
                    if (certificates.Count == 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnableToLoadCertificateIdentity")));
                    }
                    X509Certificate2 certificate = new X509Certificate2(certificates[0]);
                    if (element.CertificateReference.IsChainIncluded)
                    {
                        X509Chain certificateChain = new X509Chain {
                            ChainPolicy = { RevocationMode = X509RevocationMode.NoCheck }
                        };
                        certificateChain.Build(certificate);
                        return EndpointIdentity.CreateX509CertificateIdentity(certificateChain);
                    }
                    identity = EndpointIdentity.CreateX509CertificateIdentity(certificate);
                }
                finally
                {
                    System.ServiceModel.Security.SecurityUtils.ResetAllCertificates(certificates);
                    store.Close();
                }
            }
            return identity;
        }

        [SecuritySafeCritical]
        internal static Collection<IPolicyImportExtension> LoadPolicyImporters(PolicyImporterElementCollection policyImporterElements, ContextInformation context)
        {
            Collection<IPolicyImportExtension> collection = new Collection<IPolicyImportExtension>();
            foreach (PolicyImporterElement element in policyImporterElements)
            {
                System.Type c = System.Type.GetType(element.Type, true, true);
                if (!typeof(IPolicyImportExtension).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidPolicyExtensionTypeInConfig", new object[] { c.AssemblyQualifiedName })));
                }
                ConstructorInfo constructor = c.GetConstructor(emptyTypeArray);
                if (constructor == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("PolicyExtensionTypeRequiresDefaultConstructor", new object[] { c.AssemblyQualifiedName })));
                }
                collection.Add((IPolicyImportExtension) constructor.Invoke(emptyObjectArray));
            }
            return collection;
        }

        [SecuritySafeCritical]
        public void LoadServiceDescription(ServiceHostBase host, System.ServiceModel.Description.ServiceDescription description, ServiceElement serviceElement, Action<Uri> addBaseAddress)
        {
            CommonBehaviorsSection commonBehaviors = LookupCommonBehaviors((serviceElement == null) ? null : ConfigurationHelpers.GetEvaluationContext(serviceElement));
            if ((commonBehaviors != null) && (commonBehaviors.ServiceBehaviors != null))
            {
                LoadBehaviors<IServiceBehavior>(commonBehaviors.ServiceBehaviors, description.Behaviors, true);
            }
            string behaviorName = "";
            if (serviceElement != null)
            {
                this.LoadHostConfig(serviceElement, host, addBaseAddress);
                behaviorName = serviceElement.BehaviorConfiguration;
            }
            ServiceBehaviorElement serviceBehaviors = LookupServiceBehaviors(behaviorName, ConfigurationHelpers.GetEvaluationContext(serviceElement));
            if (serviceBehaviors != null)
            {
                LoadBehaviors<IServiceBehavior>(serviceBehaviors, description.Behaviors, false);
            }
            ServiceHostBase.ServiceAndBehaviorsContractResolver contractResolver = this.contractResolver as ServiceHostBase.ServiceAndBehaviorsContractResolver;
            if (contractResolver != null)
            {
                contractResolver.AddBehaviorContractsToResolver(description.Behaviors);
            }
            if (serviceElement != null)
            {
                foreach (ServiceEndpointElement element2 in serviceElement.Endpoints)
                {
                    if (string.IsNullOrEmpty(element2.Kind))
                    {
                        Binding binding;
                        ServiceEndpoint endpoint;
                        ContractDescription contract = this.LookupContract(element2.Contract, description.Name);
                        string key = element2.Binding + ":" + element2.BindingConfiguration;
                        if (!this.bindingTable.TryGetValue(key, out binding))
                        {
                            binding = LookupBinding(element2.Binding, element2.BindingConfiguration, ConfigurationHelpers.GetEvaluationContext(serviceElement));
                            this.bindingTable.Add(key, binding);
                        }
                        if (!string.IsNullOrEmpty(element2.BindingName))
                        {
                            binding.Name = element2.BindingName;
                        }
                        if (!string.IsNullOrEmpty(element2.BindingNamespace))
                        {
                            binding.Namespace = element2.BindingNamespace;
                        }
                        Uri address = element2.Address;
                        if (null == address)
                        {
                            endpoint = new ServiceEndpoint(contract) {
                                Binding = binding
                            };
                        }
                        else
                        {
                            Uri uri = ServiceHostBase.MakeAbsoluteUri(address, binding, host.InternalBaseAddresses);
                            endpoint = new ServiceEndpoint(contract, binding, new EndpointAddress(uri, LoadIdentity(element2.Identity), element2.Headers.Headers)) {
                                UnresolvedAddress = element2.Address
                            };
                        }
                        if (element2.ListenUri != null)
                        {
                            endpoint.ListenUri = ServiceHostBase.MakeAbsoluteUri(element2.ListenUri, binding, host.InternalBaseAddresses);
                            endpoint.UnresolvedListenUri = element2.ListenUri;
                        }
                        endpoint.ListenUriMode = element2.ListenUriMode;
                        if (!string.IsNullOrEmpty(element2.Name))
                        {
                            endpoint.Name = element2.Name;
                        }
                        KeyedByTypeCollection<IEndpointBehavior> behaviors = endpoint.Behaviors;
                        EndpointBehaviorElement endpointBehaviors = LookupEndpointBehaviors(element2.BehaviorConfiguration, ConfigurationHelpers.GetEvaluationContext(element2));
                        if (endpointBehaviors != null)
                        {
                            LoadBehaviors<IEndpointBehavior>(endpointBehaviors, behaviors, false);
                        }
                        if (element2.ElementInformation.Properties["isSystemEndpoint"].ValueOrigin != PropertyValueOrigin.Default)
                        {
                            endpoint.IsSystemEndpoint = element2.IsSystemEndpoint;
                        }
                        description.Endpoints.Add(endpoint);
                    }
                    else
                    {
                        ServiceEndpoint item = this.LookupEndpoint(element2, ConfigurationHelpers.GetEvaluationContext(serviceElement), host, description, false);
                        description.Endpoints.Add(item);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        internal static Collection<IWsdlImportExtension> LoadWsdlImporters(WsdlImporterElementCollection wsdlImporterElements, ContextInformation context)
        {
            Collection<IWsdlImportExtension> collection = new Collection<IWsdlImportExtension>();
            foreach (WsdlImporterElement element in wsdlImporterElements)
            {
                System.Type c = System.Type.GetType(element.Type, true, true);
                if (!typeof(IWsdlImportExtension).IsAssignableFrom(c))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InvalidWsdlExtensionTypeInConfig", new object[] { c.AssemblyQualifiedName })));
                }
                ConstructorInfo constructor = c.GetConstructor(emptyTypeArray);
                if (constructor == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("WsdlExtensionTypeRequiresDefaultConstructor", new object[] { c.AssemblyQualifiedName })));
                }
                collection.Add((IWsdlImportExtension) constructor.Invoke(emptyObjectArray));
            }
            return collection;
        }

        internal static Binding LookupBinding(string bindingSectionName, string configurationName)
        {
            return LookupBinding(bindingSectionName, configurationName, null);
        }

        [SecuritySafeCritical]
        internal static Binding LookupBinding(string bindingSectionName, string configurationName, ContextInformation context)
        {
            Binding binding;
            BindingCollectionElement bindingCollectionElement = GetBindingCollectionElement(bindingSectionName, context);
            if (configurationName == null)
            {
                binding = bindingCollectionElement.GetDefault();
            }
            else
            {
                Binding defaultBinding = bindingCollectionElement.GetDefault();
                binding = LookupBinding(bindingSectionName, configurationName, bindingCollectionElement, defaultBinding);
                if ((binding == null) && (configurationName == ""))
                {
                    binding = defaultBinding;
                }
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                int num;
                string str;
                Dictionary<string, object> dictionary = new Dictionary<string, object>(3);
                dictionary["FoundBinding"] = binding != null;
                if (string.IsNullOrEmpty(configurationName))
                {
                    num = 0x80025;
                    str = System.ServiceModel.SR.GetString("TraceCodeGetDefaultConfiguredBinding");
                }
                else
                {
                    num = 0x80022;
                    str = System.ServiceModel.SR.GetString("TraceCodeGetConfiguredBinding");
                    dictionary["Name"] = string.IsNullOrEmpty(configurationName) ? System.ServiceModel.SR.GetString("Default") : configurationName;
                }
                dictionary["Binding"] = bindingSectionName;
                TraceUtility.TraceEvent(TraceEventType.Verbose, num, str, new DictionaryTraceRecord(dictionary), null, null);
            }
            return binding;
        }

        private static Binding LookupBinding(string bindingSectionName, string configurationName, BindingCollectionElement bindingCollectionElement, Binding defaultBinding)
        {
            Binding binding = defaultBinding;
            if (configurationName != null)
            {
                bool flag = false;
                foreach (object obj2 in bindingCollectionElement.ConfiguredBindings)
                {
                    IBindingConfigurationElement element = obj2 as IBindingConfigurationElement;
                    if ((element != null) && element.Name.Equals(configurationName, StringComparison.Ordinal))
                    {
                        if (resolvedBindings == null)
                        {
                            resolvedBindings = new List<string>();
                        }
                        string item = bindingSectionName + "/" + configurationName;
                        if (resolvedBindings.Contains(item))
                        {
                            ConfigurationElement element2 = (ConfigurationElement) obj2;
                            StringBuilder builder = new StringBuilder();
                            foreach (string str2 in resolvedBindings)
                            {
                                builder = builder.AppendFormat("{0}, ", str2);
                            }
                            builder = builder.Append(item);
                            resolvedBindings = null;
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigBindingReferenceCycleDetected", new object[] { builder.ToString() }), element2.ElementInformation.Source, element2.ElementInformation.LineNumber));
                        }
                        try
                        {
                            CheckAccess(obj2 as IConfigurationContextProviderInternal);
                            resolvedBindings.Add(item);
                            element.ApplyConfiguration(binding);
                            resolvedBindings.Remove(item);
                        }
                        catch
                        {
                            if (resolvedBindings != null)
                            {
                                resolvedBindings = null;
                            }
                            throw;
                        }
                        if ((resolvedBindings != null) && (resolvedBindings.Count == 0))
                        {
                            resolvedBindings = null;
                        }
                        flag = true;
                    }
                }
                if (!flag)
                {
                    binding = null;
                }
            }
            return binding;
        }

        [SecurityCritical]
        private static ChannelEndpointElement LookupChannel(ContextInformation configurationContext, string configurationName, ContractDescription contract, EndpointAddress address, bool wildcard, bool useChannelElementKind, out ServiceEndpoint serviceEndpoint)
        {
            serviceEndpoint = null;
            ClientSection section = (configurationContext == null) ? ClientSection.UnsafeGetSection() : ClientSection.UnsafeGetSection(configurationContext);
            ChannelEndpointElement element = null;
            foreach (ChannelEndpointElement element2 in section.Endpoints)
            {
                ServiceEndpoint endpoint;
                if (IsChannelElementMatch(element2, contract, address, useChannelElementKind, out endpoint) && ((element2.Name == configurationName) || wildcard))
                {
                    if (element != null)
                    {
                        if (wildcard)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConfigLoaderMultipleEndpointMatchesWildcard1", new object[] { contract.ConfigurationName })));
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxConfigLoaderMultipleEndpointMatchesSpecified2", new object[] { contract.ConfigurationName, configurationName })));
                    }
                    element = element2;
                    serviceEndpoint = endpoint;
                }
            }
            if (element != null)
            {
                CheckAccess(element);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, object> dictionary = new Dictionary<string, object>(8);
                dictionary["FoundChannelElement"] = element != null;
                dictionary["Name"] = configurationName;
                dictionary["ContractName"] = contract.ConfigurationName;
                if (element != null)
                {
                    if (!string.IsNullOrEmpty(element.Binding))
                    {
                        dictionary["Binding"] = element.Binding;
                    }
                    if (!string.IsNullOrEmpty(element.BindingConfiguration))
                    {
                        dictionary["BindingConfiguration"] = element.BindingConfiguration;
                    }
                    if (element.Address != null)
                    {
                        dictionary["RemoteEndpointUri"] = element.Address.ToString();
                    }
                    if (!string.IsNullOrEmpty(element.ElementInformation.Source))
                    {
                        dictionary["ConfigurationFileSource"] = element.ElementInformation.Source;
                        dictionary["ConfigurationFileLineNumber"] = element.ElementInformation.LineNumber;
                    }
                }
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80023, System.ServiceModel.SR.GetString("TraceCodeGetChannelEndpointElement"), new DictionaryTraceRecord(dictionary), null, null);
            }
            return element;
        }

        internal static ComContractElement LookupComContract(Guid contractIID)
        {
            ComContractsSection section = (ComContractsSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ComContractsSectionPath);
            foreach (ComContractElement element in section.ComContracts)
            {
                Guid guid;
                if (DiagnosticUtility.Utility.TryCreateGuid(element.Contract, out guid) && (guid == contractIID))
                {
                    return element;
                }
            }
            return null;
        }

        [SecurityCritical]
        private static CommonBehaviorsSection LookupCommonBehaviors(ContextInformation context)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80021, System.ServiceModel.SR.GetString("TraceCodeGetCommonBehaviors"), null);
            }
            if (context != null)
            {
                return CommonBehaviorsSection.UnsafeGetAssociatedSection(context);
            }
            return CommonBehaviorsSection.UnsafeGetSection();
        }

        internal ContractDescription LookupContract(string contractName, string serviceName)
        {
            ContractDescription contractForStandardEndpoint = this.LookupContractForStandardEndpoint(contractName, serviceName);
            if (contractForStandardEndpoint != null)
            {
                return contractForStandardEndpoint;
            }
            if (contractName == string.Empty)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFoundEmpty", new object[] { serviceName })));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFound2", new object[] { contractName, serviceName })));
        }

        internal ContractDescription LookupContractForStandardEndpoint(string contractName, string serviceName)
        {
            ContractDescription description = this.contractResolver.ResolveContract(contractName);
            if ((description == null) && (contractName == "IMetadataExchange"))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFoundIMetadataExchange", new object[] { serviceName })));
            }
            return description;
        }

        internal static ServiceEndpoint LookupEndpoint(ChannelEndpointElement channelEndpointElement, ContextInformation context)
        {
            return LookupEndpoint(channelEndpointElement, context, null, null);
        }

        [SecuritySafeCritical]
        internal static ServiceEndpoint LookupEndpoint(string configurationName, EndpointAddress address, ContractDescription contract)
        {
            return LookupEndpoint(configurationName, address, contract, null);
        }

        [SecuritySafeCritical]
        private static ServiceEndpoint LookupEndpoint(ChannelEndpointElement channelEndpointElement, ContextInformation context, EndpointAddress address, ContractDescription contract)
        {
            EndpointCollectionElement endpointCollectionElement = LookupEndpointCollectionElement(channelEndpointElement.Kind, context);
            ServiceEndpoint endpoint = null;
            string str = channelEndpointElement.EndpointConfiguration ?? string.Empty;
            bool flag = false;
            foreach (StandardEndpointElement element2 in endpointCollectionElement.ConfiguredEndpoints)
            {
                if (element2.Name.Equals(str, StringComparison.Ordinal))
                {
                    if (resolvedEndpoints == null)
                    {
                        resolvedEndpoints = new List<string>();
                    }
                    string item = channelEndpointElement.Kind + "/" + str;
                    if (resolvedEndpoints.Contains(item))
                    {
                        ConfigurationElement element3 = element2;
                        StringBuilder builder = new StringBuilder();
                        foreach (string str3 in resolvedEndpoints)
                        {
                            builder = builder.AppendFormat("{0}, ", str3);
                        }
                        builder = builder.Append(item);
                        resolvedEndpoints = null;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointReferenceCycleDetected", new object[] { builder.ToString() }), element3.ElementInformation.Source, element3.ElementInformation.LineNumber));
                    }
                    try
                    {
                        CheckAccess(element2);
                        resolvedEndpoints.Add(item);
                        ConfigureEndpoint(element2, channelEndpointElement, address, context, contract, out endpoint);
                        resolvedEndpoints.Remove(item);
                    }
                    catch
                    {
                        if (resolvedEndpoints != null)
                        {
                            resolvedBindings = null;
                        }
                        throw;
                    }
                    if ((resolvedEndpoints != null) && (resolvedEndpoints.Count == 0))
                    {
                        resolvedEndpoints = null;
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                endpoint = null;
            }
            if ((endpoint == null) && string.IsNullOrEmpty(str))
            {
                ConfigureEndpoint(endpointCollectionElement.GetDefaultStandardEndpointElement(), channelEndpointElement, address, context, contract, out endpoint);
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                int num;
                string str4;
                Dictionary<string, object> dictionary = new Dictionary<string, object>(3);
                dictionary["FoundEndpoint"] = endpoint != null;
                if (string.IsNullOrEmpty(str))
                {
                    num = 0x80044;
                    str4 = System.ServiceModel.SR.GetString("TraceCodeGetDefaultConfiguredEndpoint");
                }
                else
                {
                    num = 0x80043;
                    str4 = System.ServiceModel.SR.GetString("TraceCodeGetConfiguredEndpoint");
                    dictionary["Name"] = str;
                }
                dictionary["Endpoint"] = channelEndpointElement.Kind;
                TraceUtility.TraceEvent(TraceEventType.Verbose, num, str4, new DictionaryTraceRecord(dictionary), null, null);
            }
            if (endpoint != null)
            {
                endpoint.IsFullyConfigured = true;
            }
            return endpoint;
        }

        [SecuritySafeCritical]
        internal static ServiceEndpoint LookupEndpoint(string configurationName, EndpointAddress address, ContractDescription contract, ContextInformation configurationContext)
        {
            ServiceEndpoint endpoint;
            bool wildcard = IsWildcardMatch(configurationName);
            LookupChannel(configurationContext, configurationName, contract, address, wildcard, true, out endpoint);
            return endpoint;
        }

        [SecuritySafeCritical]
        internal ServiceEndpoint LookupEndpoint(ServiceEndpointElement serviceEndpointElement, ContextInformation context, ServiceHostBase host, System.ServiceModel.Description.ServiceDescription description, bool omitSettingEndpointAddress = false)
        {
            EndpointCollectionElement endpointCollectionElement = LookupEndpointCollectionElement(serviceEndpointElement.Kind, context);
            ServiceEndpoint endpoint = null;
            string str = serviceEndpointElement.EndpointConfiguration ?? string.Empty;
            bool flag = false;
            foreach (StandardEndpointElement element2 in endpointCollectionElement.ConfiguredEndpoints)
            {
                if (element2.Name.Equals(str, StringComparison.Ordinal))
                {
                    if (resolvedEndpoints == null)
                    {
                        resolvedEndpoints = new List<string>();
                    }
                    string item = serviceEndpointElement.Kind + "/" + str;
                    if (resolvedEndpoints.Contains(item))
                    {
                        ConfigurationElement element3 = element2;
                        StringBuilder builder = new StringBuilder();
                        foreach (string str3 in resolvedEndpoints)
                        {
                            builder = builder.AppendFormat("{0}, ", str3);
                        }
                        builder = builder.Append(item);
                        resolvedEndpoints = null;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointReferenceCycleDetected", new object[] { builder.ToString() }), element3.ElementInformation.Source, element3.ElementInformation.LineNumber));
                    }
                    try
                    {
                        CheckAccess(element2);
                        resolvedEndpoints.Add(item);
                        this.ConfigureEndpoint(element2, serviceEndpointElement, context, host, description, out endpoint, false);
                        resolvedEndpoints.Remove(item);
                    }
                    catch
                    {
                        if (resolvedEndpoints != null)
                        {
                            resolvedBindings = null;
                        }
                        throw;
                    }
                    if ((resolvedEndpoints != null) && (resolvedEndpoints.Count == 0))
                    {
                        resolvedEndpoints = null;
                    }
                    flag = true;
                }
            }
            if (!flag)
            {
                endpoint = null;
            }
            if ((endpoint == null) && string.IsNullOrEmpty(str))
            {
                StandardEndpointElement defaultStandardEndpointElement = endpointCollectionElement.GetDefaultStandardEndpointElement();
                this.ConfigureEndpoint(defaultStandardEndpointElement, serviceEndpointElement, context, host, description, out endpoint, omitSettingEndpointAddress);
            }
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                int num;
                string str4;
                Dictionary<string, object> dictionary = new Dictionary<string, object>(3);
                dictionary["FoundEndpoint"] = endpoint != null;
                if (string.IsNullOrEmpty(str))
                {
                    num = 0x80044;
                    str4 = System.ServiceModel.SR.GetString("TraceCodeGetDefaultConfiguredEndpoint");
                }
                else
                {
                    num = 0x80043;
                    str4 = System.ServiceModel.SR.GetString("TraceCodeGetConfiguredEndpoint");
                    dictionary["Name"] = str;
                }
                dictionary["Endpoint"] = serviceEndpointElement.Kind;
                TraceUtility.TraceEvent(TraceEventType.Verbose, num, str4, new DictionaryTraceRecord(dictionary), null, null);
            }
            return endpoint;
        }

        [SecurityCritical]
        private static EndpointBehaviorElement LookupEndpointBehaviors(string behaviorName, ContextInformation context)
        {
            EndpointBehaviorElement element = null;
            if (behaviorName != null)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80020, System.ServiceModel.SR.GetString("TraceCodeGetBehaviorElement"), new StringTraceRecord("BehaviorName", behaviorName), null, null);
                }
                BehaviorsSection section = null;
                if (context == null)
                {
                    section = BehaviorsSection.UnsafeGetSection();
                }
                else
                {
                    section = BehaviorsSection.UnsafeGetAssociatedSection(context);
                }
                if (section.EndpointBehaviors.ContainsKey(behaviorName))
                {
                    element = section.EndpointBehaviors[behaviorName];
                }
            }
            if (element != null)
            {
                CheckAccess(element);
            }
            return element;
        }

        [SecurityCritical]
        private static EndpointCollectionElement LookupEndpointCollectionElement(string endpointSectionName, ContextInformation context)
        {
            if (string.IsNullOrEmpty(endpointSectionName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigEndpointTypeCannotBeNullOrEmpty")));
            }
            if (context == null)
            {
                return ConfigurationHelpers.UnsafeGetEndpointCollectionElement(endpointSectionName);
            }
            return ConfigurationHelpers.UnsafeGetAssociatedEndpointCollectionElement(context, endpointSectionName);
        }

        [SecuritySafeCritical]
        internal static ProtocolMappingItem LookupProtocolMapping(string scheme)
        {
            ProtocolMappingSection section = (ProtocolMappingSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ProtocolMappingSectionPath);
            foreach (ProtocolMappingElement element in section.ProtocolMappingCollection)
            {
                if (element.Scheme == scheme)
                {
                    return new ProtocolMappingItem(element.Binding, element.BindingConfiguration);
                }
            }
            return null;
        }

        [SecurityCritical]
        public ServiceElement LookupService(string serviceConfigurationName)
        {
            ServicesSection section = ServicesSection.UnsafeGetSection();
            ServiceElement element = null;
            ServiceElementCollection services = section.Services;
            for (int i = 0; i < services.Count; i++)
            {
                ServiceElement element2 = services[i];
                if (element2.Name == serviceConfigurationName)
                {
                    element = element2;
                }
            }
            if (element != null)
            {
                CheckAccess(element);
            }
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80026, System.ServiceModel.SR.GetString("TraceCodeGetServiceElement"), new ServiceConfigurationTraceRecord(element), null, null);
            }
            return element;
        }

        [SecurityCritical]
        private static ServiceBehaviorElement LookupServiceBehaviors(string behaviorName, ContextInformation context)
        {
            ServiceBehaviorElement element = null;
            if (behaviorName != null)
            {
                if (DiagnosticUtility.ShouldTraceVerbose)
                {
                    TraceUtility.TraceEvent(TraceEventType.Verbose, 0x80020, System.ServiceModel.SR.GetString("TraceCodeGetBehaviorElement"), new StringTraceRecord("BehaviorName", behaviorName), null, null);
                }
                BehaviorsSection section = null;
                if (context == null)
                {
                    section = BehaviorsSection.UnsafeGetSection();
                }
                else
                {
                    section = BehaviorsSection.UnsafeGetAssociatedSection(context);
                }
                if (section.ServiceBehaviors.ContainsKey(behaviorName))
                {
                    element = section.ServiceBehaviors[behaviorName];
                }
            }
            if (element != null)
            {
                CheckAccess(element);
            }
            return element;
        }

        [SecurityCritical]
        private static bool ShouldSkipCommonBehavior(System.Type behaviorType, ref bool? isPT)
        {
            bool flag = false;
            if (!isPT.HasValue)
            {
                if (!PartialTrustHelpers.IsTypeAptca(behaviorType))
                {
                    isPT = new bool?(!ThreadHasConfigurationPermission());
                    flag = isPT.Value;
                }
                return flag;
            }
            if (isPT.Value)
            {
                flag = !PartialTrustHelpers.IsTypeAptca(behaviorType);
            }
            return flag;
        }

        [SecurityCritical]
        private static bool ThreadHasConfigurationPermission()
        {
            try
            {
                ConfigurationPermission.Demand();
            }
            catch (SecurityException)
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void TraceBehaviorWarning(BehaviorExtensionElement behaviorExtension, int traceCode, string traceDescription, System.Type type, System.Type behaviorType)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Hashtable hashtable2 = new Hashtable(3);
                hashtable2.Add("ConfigurationElementName", behaviorExtension.ConfigurationElementName);
                hashtable2.Add("ConfigurationType", type.AssemblyQualifiedName);
                hashtable2.Add("BehaviorType", behaviorType.AssemblyQualifiedName);
                Hashtable dictionary = hashtable2;
                TraceUtility.TraceEvent(TraceEventType.Warning, traceCode, traceDescription, new DictionaryTraceRecord(dictionary), null, null);
            }
        }

        private static System.Configuration.ConfigurationPermission ConfigurationPermission
        {
            [SecuritySafeCritical]
            get
            {
                if (configurationPermission == null)
                {
                    configurationPermission = new System.Configuration.ConfigurationPermission(PermissionState.Unrestricted);
                }
                return configurationPermission;
            }
        }
    }
}

