namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class DataContractSerializerServiceBehavior : IServiceBehavior, IEndpointBehavior
    {
        private bool ignoreExtensionDataObject;
        private int maxItemsInObjectGraph;

        internal DataContractSerializerServiceBehavior(bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            this.ignoreExtensionDataObject = ignoreExtensionDataObject;
            this.maxItemsInObjectGraph = maxItemsInObjectGraph;
        }

        internal static void ApplySerializationSettings(System.ServiceModel.Description.ServiceDescription description, bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (!endpoint.InternalIsSystemEndpoint(description))
                {
                    ApplySerializationSettings(endpoint, ignoreExtensionDataObject, maxItemsInObjectGraph);
                }
            }
        }

        internal static void ApplySerializationSettings(ServiceEndpoint endpoint, bool ignoreExtensionDataObject, int maxItemsInObjectGraph)
        {
            foreach (OperationDescription description in endpoint.Contract.Operations)
            {
                foreach (IOperationBehavior behavior in description.Behaviors)
                {
                    if (behavior is DataContractSerializerOperationBehavior)
                    {
                        DataContractSerializerOperationBehavior behavior2 = (DataContractSerializerOperationBehavior) behavior;
                        if (behavior2 != null)
                        {
                            if (!behavior2.IgnoreExtensionDataObjectSetExplicit)
                            {
                                behavior2.ignoreExtensionDataObject = ignoreExtensionDataObject;
                            }
                            if (!behavior2.MaxItemsInObjectGraphSetExplicit)
                            {
                                behavior2.maxItemsInObjectGraph = maxItemsInObjectGraph;
                            }
                        }
                    }
                }
            }
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            ApplySerializationSettings(serviceEndpoint, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            ApplySerializationSettings(serviceEndpoint, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            ApplySerializationSettings(description, this.ignoreExtensionDataObject, this.maxItemsInObjectGraph);
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public bool IgnoreExtensionDataObject
        {
            get
            {
                return this.ignoreExtensionDataObject;
            }
            set
            {
                this.ignoreExtensionDataObject = value;
            }
        }

        public int MaxItemsInObjectGraph
        {
            get
            {
                return this.maxItemsInObjectGraph;
            }
            set
            {
                this.maxItemsInObjectGraph = value;
            }
        }
    }
}

