namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public interface IEndpointBehavior
    {
        void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters);
        void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime);
        void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher);
        void Validate(ServiceEndpoint endpoint);
    }
}

