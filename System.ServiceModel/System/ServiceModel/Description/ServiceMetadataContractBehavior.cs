namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public sealed class ServiceMetadataContractBehavior : IContractBehavior
    {
        private bool metadataGenerationDisabled;

        public ServiceMetadataContractBehavior()
        {
        }

        public ServiceMetadataContractBehavior(bool metadataGenerationDisabled) : this()
        {
            this.metadataGenerationDisabled = metadataGenerationDisabled;
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
        }

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }

        public bool MetadataGenerationDisabled
        {
            get
            {
                return this.metadataGenerationDisabled;
            }
            set
            {
                this.metadataGenerationDisabled = value;
            }
        }
    }
}

