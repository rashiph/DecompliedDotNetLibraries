namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public sealed class BufferedReceiveServiceBehavior : IServiceBehavior
    {
        internal const int DefaultMaxPendingMessagesPerChannel = 0x200;
        private int maxPendingMessagesPerChannel = 0x200;

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase is WorkflowServiceHost)
            {
                foreach (ChannelDispatcherBase base2 in serviceHostBase.ChannelDispatchers)
                {
                    ChannelDispatcher dispatcher = base2 as ChannelDispatcher;
                    if (dispatcher != null)
                    {
                        foreach (EndpointDispatcher dispatcher2 in dispatcher.Endpoints)
                        {
                            if (WorkflowServiceBehavior.IsWorkflowEndpoint(dispatcher2))
                            {
                                dispatcher2.DispatchRuntime.PreserveMessage = true;
                                foreach (DispatchOperation operation in dispatcher2.DispatchRuntime.Operations)
                                {
                                    operation.BufferedReceiveEnabled = true;
                                }
                            }
                        }
                    }
                }
                serviceHostBase.Extensions.Add(new BufferedReceiveManager(this.MaxPendingMessagesPerChannel));
            }
        }

        internal static bool IsWorkflowEndpoint(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint.IsSystemEndpoint)
            {
                return false;
            }
            foreach (OperationDescription description in serviceEndpoint.Contract.Operations)
            {
                if (description.Behaviors.Find<WorkflowOperationBehavior>() == null)
                {
                    return false;
                }
            }
            return true;
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                if (IsWorkflowEndpoint(endpoint))
                {
                    foreach (OperationDescription description in endpoint.Contract.Operations)
                    {
                        ReceiveContextEnabledAttribute attribute = description.Behaviors.Find<ReceiveContextEnabledAttribute>();
                        if ((attribute == null) || !attribute.ManualControl)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.BufferedReceiveRequiresReceiveContext(description.Name)));
                        }
                    }
                }
            }
        }

        public int MaxPendingMessagesPerChannel
        {
            get
            {
                return this.maxPendingMessagesPerChannel;
            }
            set
            {
                if (value <= 0)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, System.ServiceModel.Activities.SR.MaxPendingMessagesPerChannelMustBeGreaterThanZero);
                }
                this.maxPendingMessagesPerChannel = value;
            }
        }
    }
}

