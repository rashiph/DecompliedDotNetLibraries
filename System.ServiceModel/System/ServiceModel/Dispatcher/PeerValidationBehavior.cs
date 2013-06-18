namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class PeerValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        private static PeerValidationBehavior instance;

        private PeerValidationBehavior()
        {
        }

        private static bool IsRequestReplyContract(ContractDescription contract)
        {
            foreach (OperationDescription description in contract.Operations)
            {
                if (description.Messages.Count > 1)
                {
                    return true;
                }
            }
            return false;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }
            ContractDescription contract = serviceEndpoint.Contract;
            Binding binding = serviceEndpoint.Binding;
            this.ValidateHelper(contract, binding);
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = description.Endpoints[i];
                this.ValidateHelper(endpoint.Contract, endpoint.Binding);
            }
        }

        private void ValidateHelper(ContractDescription contract, Binding binding)
        {
            if ((binding is NetPeerTcpBinding) && IsRequestReplyContract(contract))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BindingDoesnTSupportRequestReplyButContract1", new object[] { binding.Name })));
            }
        }

        public static PeerValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PeerValidationBehavior();
                }
                return instance;
            }
        }
    }
}

