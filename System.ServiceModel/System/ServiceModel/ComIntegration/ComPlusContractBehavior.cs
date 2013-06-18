namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class ComPlusContractBehavior : IContractBehavior
    {
        private ServiceInfo info;

        public ComPlusContractBehavior(ServiceInfo info)
        {
            this.info = info;
        }

        public void AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
            dispatch.InstanceProvider = new ComPlusInstanceProvider(this.info);
            dispatch.InstanceContextInitializers.Add(new ComPlusInstanceContextInitializer(this.info));
            foreach (DispatchOperation operation in dispatch.Operations)
            {
                operation.CallContextInitializers.Add(new ComPlusThreadInitializer(description, operation, this.info));
            }
        }

        public void Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }
    }
}

