namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    public class ServiceHost : ServiceHostBase
    {
        private IDisposable disposableInstance;
        private ReflectedContractCollection reflectedContracts;
        private System.Type serviceType;
        private object singletonInstance;

        protected ServiceHost()
        {
        }

        public ServiceHost(object singletonInstance, params Uri[] baseAddresses)
        {
            if (singletonInstance == null)
            {
                throw new ArgumentNullException("singletonInstance");
            }
            this.singletonInstance = singletonInstance;
            this.serviceType = singletonInstance.GetType();
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructServiceHost", new object[] { this.serviceType.FullName }), ActivityType.Construct);
                }
                this.InitializeDescription(singletonInstance, new UriSchemeKeyedCollection(baseAddresses));
            }
        }

        public ServiceHost(System.Type serviceType, params Uri[] baseAddresses)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceType"));
            }
            this.serviceType = serviceType;
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructServiceHost", new object[] { serviceType.FullName }), ActivityType.Construct);
                }
                this.InitializeDescription(serviceType, new UriSchemeKeyedCollection(baseAddresses));
            }
        }

        internal override void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
            List<ContractDescription> list = new List<ContractDescription>();
            for (int i = 0; i < this.reflectedContracts.Count; i++)
            {
                bool flag = true;
                ContractDescription item = this.reflectedContracts[i];
                System.Type contractType = item.ContractType;
                if (contractType != null)
                {
                    for (int j = 0; j < this.reflectedContracts.Count; j++)
                    {
                        ContractDescription description2 = this.reflectedContracts[j];
                        System.Type c = description2.ContractType;
                        if (((i != j) && (c != null)) && contractType.IsAssignableFrom(c))
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    list.Add(item);
                }
            }
            foreach (ContractDescription description3 in list)
            {
                ServiceEndpoint endpoint = base.AddServiceEndpoint(description3.ConfigurationName, defaultBinding, string.Empty);
                ConfigLoader.LoadDefaultEndpointBehaviors(endpoint);
                defaultEndpoints.Add(endpoint);
            }
        }

        public ServiceEndpoint AddServiceEndpoint(System.Type implementedContract, Binding binding, string address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, null);
        }

        public ServiceEndpoint AddServiceEndpoint(System.Type implementedContract, Binding binding, Uri address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, null);
        }

        public ServiceEndpoint AddServiceEndpoint(System.Type implementedContract, Binding binding, string address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }
            ServiceEndpoint endpoint = this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute));
            if (listenUri != null)
            {
                listenUri = base.MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        public ServiceEndpoint AddServiceEndpoint(System.Type implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            if (implementedContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("implementedContract"));
            }
            if (this.reflectedContracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractsNotInitialized1", new object[] { implementedContract.FullName })));
            }
            ReflectedAndBehaviorContractCollection reflectedAndBehaviorContracts = new ReflectedAndBehaviorContractCollection(this.reflectedContracts, base.Description.Behaviors);
            this.ValidateContractType(implementedContract, reflectedAndBehaviorContracts);
            ServiceEndpoint endpoint = base.AddServiceEndpoint(reflectedAndBehaviorContracts.GetConfigKey(implementedContract), binding, address);
            if (listenUri != null)
            {
                listenUri = base.MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            System.ServiceModel.Description.ServiceDescription service;
            if (this.serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostCannotCreateDescriptionWithoutServiceType")));
            }
            if (this.SingletonInstance != null)
            {
                service = System.ServiceModel.Description.ServiceDescription.GetService(this.SingletonInstance);
            }
            else
            {
                service = System.ServiceModel.Description.ServiceDescription.GetService(this.serviceType);
            }
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
            object wellKnownSingleton = attribute.GetWellKnownSingleton();
            if (wellKnownSingleton == null)
            {
                wellKnownSingleton = attribute.GetHiddenSingleton();
                this.disposableInstance = wellKnownSingleton as IDisposable;
            }
            if ((typeof(IServiceBehavior).IsAssignableFrom(this.serviceType) || typeof(IContractBehavior).IsAssignableFrom(this.serviceType)) && (wellKnownSingleton == null))
            {
                wellKnownSingleton = System.ServiceModel.Description.ServiceDescription.CreateImplementation(this.serviceType);
                this.disposableInstance = wellKnownSingleton as IDisposable;
            }
            if ((this.SingletonInstance == null) && (wellKnownSingleton is IServiceBehavior))
            {
                service.Behaviors.Add((IServiceBehavior) wellKnownSingleton);
            }
            ReflectedContractCollection contracts = new ReflectedContractCollection();
            List<System.Type> interfaces = ServiceReflector.GetInterfaces(this.serviceType);
            for (int i = 0; i < interfaces.Count; i++)
            {
                System.Type key = interfaces[i];
                if (!contracts.Contains(key))
                {
                    ContractDescription item = null;
                    if (wellKnownSingleton != null)
                    {
                        item = ContractDescription.GetContract(key, wellKnownSingleton);
                    }
                    else
                    {
                        item = ContractDescription.GetContract(key, this.serviceType);
                    }
                    contracts.Add(item);
                    Collection<ContractDescription> inheritedContracts = item.GetInheritedContracts();
                    for (int j = 0; j < inheritedContracts.Count; j++)
                    {
                        ContractDescription description3 = inheritedContracts[j];
                        if (!contracts.Contains(description3.ContractType))
                        {
                            contracts.Add(description3);
                        }
                    }
                }
            }
            this.reflectedContracts = contracts;
            implementedContracts = contracts.ToImplementedContracts();
            return service;
        }

        protected void InitializeDescription(object singletonInstance, UriSchemeKeyedCollection baseAddresses)
        {
            if (singletonInstance == null)
            {
                throw new ArgumentNullException("singletonInstance");
            }
            this.singletonInstance = singletonInstance;
            this.InitializeDescription(singletonInstance.GetType(), baseAddresses);
        }

        protected void InitializeDescription(System.Type serviceType, UriSchemeKeyedCollection baseAddresses)
        {
            if (serviceType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceType"));
            }
            this.serviceType = serviceType;
            base.InitializeDescription(baseAddresses);
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (this.disposableInstance != null)
            {
                this.disposableInstance.Dispose();
            }
        }

        private void ValidateContractType(System.Type implementedContract, ReflectedAndBehaviorContractCollection reflectedAndBehaviorContracts)
        {
            if (!implementedContract.IsDefined(typeof(ServiceContractAttribute), false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxServiceContractAttributeNotFound", new object[] { implementedContract.FullName })));
            }
            if (!reflectedAndBehaviorContracts.Contains(implementedContract))
            {
                if (implementedContract == typeof(IMetadataExchange))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFoundIMetadataExchange", new object[] { this.serviceType.FullName })));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFound2", new object[] { implementedContract.FullName, this.serviceType.FullName })));
            }
        }

        internal override string CloseActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityCloseServiceHost", new object[] { this.serviceType.FullName });
            }
        }

        internal override object DisposableInstance
        {
            get
            {
                return this.disposableInstance;
            }
        }

        internal override string OpenActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityOpenServiceHost", new object[] { this.serviceType.FullName });
            }
        }

        public object SingletonInstance
        {
            get
            {
                return this.singletonInstance;
            }
        }

        private class ReflectedAndBehaviorContractCollection
        {
            private KeyedByTypeCollection<IServiceBehavior> behaviors;
            private ServiceHost.ReflectedContractCollection reflectedContracts;

            public ReflectedAndBehaviorContractCollection(ServiceHost.ReflectedContractCollection reflectedContracts, KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                this.reflectedContracts = reflectedContracts;
                this.behaviors = behaviors;
            }

            internal bool Contains(System.Type implementedContract)
            {
                return (this.reflectedContracts.Contains(implementedContract) || (this.behaviors.Contains(typeof(ServiceMetadataBehavior)) && ServiceMetadataBehavior.IsMetadataImplementedType(implementedContract)));
            }

            internal string GetConfigKey(System.Type implementedContract)
            {
                if (this.reflectedContracts.Contains(implementedContract))
                {
                    return ServiceHost.ReflectedContractCollection.GetConfigKey(this.reflectedContracts[implementedContract]);
                }
                if (!this.behaviors.Contains(typeof(ServiceMetadataBehavior)) || !ServiceMetadataBehavior.IsMetadataImplementedType(implementedContract))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxReflectedContractKeyNotFound2", new object[] { implementedContract.FullName, string.Empty })));
                }
                return "IMetadataExchange";
            }
        }

        private class ReflectedContractCollection : KeyedCollection<System.Type, ContractDescription>
        {
            public ReflectedContractCollection() : base(null, 4)
            {
            }

            internal static string GetConfigKey(ContractDescription contract)
            {
                return contract.ConfigurationName;
            }

            protected override System.Type GetKeyForItem(ContractDescription item)
            {
                if (item == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
                }
                return item.ContractType;
            }

            public IDictionary<string, ContractDescription> ToImplementedContracts()
            {
                Dictionary<string, ContractDescription> dictionary = new Dictionary<string, ContractDescription>();
                foreach (ContractDescription description in base.Items)
                {
                    dictionary.Add(GetConfigKey(description), description);
                }
                return dictionary;
            }
        }
    }
}

