namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class MustUnderstandBehavior : IEndpointBehavior
    {
        private bool validateMustUnderstand;

        public MustUnderstandBehavior(bool validate)
        {
            this.validateMustUnderstand = validate;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (behavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("behavior"));
            }
            behavior.ValidateMustUnderstand = this.ValidateMustUnderstand;
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endpointDispatcher"));
            }
            endpointDispatcher.DispatchRuntime.ValidateMustUnderstand = this.ValidateMustUnderstand;
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public bool ValidateMustUnderstand
        {
            get
            {
                return this.validateMustUnderstand;
            }
            set
            {
                this.validateMustUnderstand = value;
            }
        }
    }
}

