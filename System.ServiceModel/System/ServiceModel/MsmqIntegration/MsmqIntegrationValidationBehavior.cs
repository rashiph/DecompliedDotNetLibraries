namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class MsmqIntegrationValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        private static MsmqIntegrationValidationBehavior instance;

        private MsmqIntegrationValidationBehavior()
        {
        }

        private bool NeedValidateBinding(Binding binding)
        {
            if (binding is MsmqIntegrationBinding)
            {
                return true;
            }
            if (binding is CustomBinding)
            {
                CustomBinding binding2 = new CustomBinding(binding);
                return (binding2.Elements.Find<MsmqIntegrationBindingElement>() != null);
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
            if (this.NeedValidateBinding(binding))
            {
                this.ValidateHelper(contract, binding, null);
            }
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
                if (this.NeedValidateBinding(endpoint.Binding))
                {
                    this.ValidateHelper(endpoint.Contract, endpoint.Binding, description);
                    return;
                }
            }
        }

        private void ValidateHelper(ContractDescription contract, Binding binding, System.ServiceModel.Description.ServiceDescription description)
        {
            foreach (OperationDescription description2 in contract.Operations)
            {
                MessageDescription description3 = description2.Messages[0];
                if ((description3.Body.Parts.Count != 0) || (description3.Headers.Count != 0))
                {
                    if (description3.Body.Parts.Count == 1)
                    {
                        System.Type type = description3.Body.Parts[0].Type;
                        if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(MsmqMessage<>)))
                        {
                            continue;
                        }
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqInvalidServiceOperationForMsmqIntegrationBinding", new object[] { binding.Name, description2.Name, contract.Name })));
                }
            }
        }

        internal static MsmqIntegrationValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MsmqIntegrationValidationBehavior();
                }
                return instance;
            }
        }
    }
}

