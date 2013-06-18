namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=true)]
    public sealed class DeliveryRequirementsAttribute : Attribute, IContractBehavior, IContractBehaviorAttribute
    {
        private System.Type contractType;
        private QueuedDeliveryRequirementsMode queuedDeliveryRequirements;
        private bool requireOrderedDelivery;

        private void EnsureOrderedDeliveryRequirements(string name, Binding binding)
        {
            if (this.RequireOrderedDelivery)
            {
                IBindingDeliveryCapabilities property = binding.GetProperty<IBindingDeliveryCapabilities>(new BindingParameterCollection());
                if (property == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SinceTheBindingForDoesnTSupportIBindingCapabilities1_1", new object[] { name })));
                }
                if (!property.AssuresOrderedDelivery)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("TheBindingForDoesnTSupportOrderedDelivery1", new object[] { name })));
                }
            }
        }

        private void EnsureQueuedDeliveryRequirements(string name, Binding binding)
        {
            if ((this.QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.Required) || (this.QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.NotAllowed))
            {
                IBindingDeliveryCapabilities property = binding.GetProperty<IBindingDeliveryCapabilities>(new BindingParameterCollection());
                if (property == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SinceTheBindingForDoesnTSupportIBindingCapabilities2_1", new object[] { name })));
                }
                bool queuedDelivery = property.QueuedDelivery;
                if ((this.QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.Required) && !queuedDelivery)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BindingRequirementsAttributeRequiresQueuedDelivery1", new object[] { name })));
                }
                if ((this.QueuedDeliveryRequirements == QueuedDeliveryRequirementsMode.NotAllowed) && queuedDelivery)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("BindingRequirementsAttributeDisallowsQueuedDelivery1", new object[] { name })));
                }
            }
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
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.ValidateEndpoint(endpoint);
        }

        private void ValidateEndpoint(ServiceEndpoint endpoint)
        {
            string name = endpoint.Contract.ContractType.Name;
            this.EnsureQueuedDeliveryRequirements(name, endpoint.Binding);
            this.EnsureOrderedDeliveryRequirements(name, endpoint.Binding);
        }

        public QueuedDeliveryRequirementsMode QueuedDeliveryRequirements
        {
            get
            {
                return this.queuedDeliveryRequirements;
            }
            set
            {
                if (!QueuedDeliveryRequirementsModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.queuedDeliveryRequirements = value;
            }
        }

        public bool RequireOrderedDelivery
        {
            get
            {
                return this.requireOrderedDelivery;
            }
            set
            {
                this.requireOrderedDelivery = value;
            }
        }

        public System.Type TargetContract
        {
            get
            {
                return this.contractType;
            }
            set
            {
                this.contractType = value;
            }
        }
    }
}

