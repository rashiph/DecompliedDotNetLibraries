namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class WorkflowServiceBehavior : IServiceBehavior
    {
        public WorkflowServiceBehavior(System.Activities.Activity activity)
        {
            this.Activity = activity;
        }

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceDescription");
            }
            if (serviceHostBase == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
            DurableInstanceContextProvider provider = new DurableInstanceContextProvider(serviceHostBase);
            DurableInstanceProvider provider2 = new DurableInstanceProvider(serviceHostBase);
            ServiceDebugBehavior behavior = serviceDescription.Behaviors.Find<ServiceDebugBehavior>();
            bool includeExceptionDetailInFaults = (behavior != null) ? behavior.IncludeExceptionDetailInFaults : false;
            foreach (ChannelDispatcherBase base2 in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher dispatcher = base2 as ChannelDispatcher;
                if (dispatcher != null)
                {
                    foreach (EndpointDispatcher dispatcher2 in dispatcher.Endpoints)
                    {
                        if (IsWorkflowEndpoint(dispatcher2))
                        {
                            DispatchRuntime dispatchRuntime = dispatcher2.DispatchRuntime;
                            dispatchRuntime.AutomaticInputSessionShutdown = true;
                            dispatchRuntime.ConcurrencyMode = ConcurrencyMode.Multiple;
                            dispatchRuntime.InstanceContextProvider = provider;
                            dispatchRuntime.InstanceProvider = provider2;
                            if (includeExceptionDetailInFaults)
                            {
                                dispatchRuntime.SetDebugFlagInDispatchOperations(includeExceptionDetailInFaults);
                            }
                        }
                    }
                }
            }
        }

        internal static bool IsWorkflowEndpoint(EndpointDispatcher endpointDispatcher)
        {
            if (!endpointDispatcher.IsSystemEndpoint)
            {
                return true;
            }
            ServiceHostBase host = endpointDispatcher.ChannelDispatcher.Host;
            ServiceEndpoint endpoint = null;
            foreach (ServiceEndpoint endpoint2 in host.Description.Endpoints)
            {
                if (endpoint2.Id == endpointDispatcher.Id)
                {
                    endpoint = endpoint2;
                    break;
                }
            }
            if (endpoint == null)
            {
                return false;
            }
            return ((endpoint is WorkflowHostingEndpoint) || endpoint.Contract.Behaviors.Contains(typeof(WorkflowContractBehaviorAttribute)));
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceDescription");
            }
            if (serviceHostBase == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
        }

        public System.Activities.Activity Activity { get; private set; }
    }
}

