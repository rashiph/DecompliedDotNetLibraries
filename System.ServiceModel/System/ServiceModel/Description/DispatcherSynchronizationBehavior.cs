namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class DispatcherSynchronizationBehavior : IEndpointBehavior
    {
        public DispatcherSynchronizationBehavior() : this(false, 1)
        {
        }

        public DispatcherSynchronizationBehavior(bool asynchronousSendEnabled, int maxPendingReceives)
        {
            this.AsynchronousSendEnabled = asynchronousSendEnabled;
            this.MaxPendingReceives = maxPendingReceives;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection parameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointDispatcher");
            }
            endpointDispatcher.ChannelDispatcher.SendAsynchronously = this.AsynchronousSendEnabled;
            endpointDispatcher.ChannelDispatcher.MaxPendingReceives = this.MaxPendingReceives;
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public bool AsynchronousSendEnabled { get; set; }

        public int MaxPendingReceives { get; set; }
    }
}

