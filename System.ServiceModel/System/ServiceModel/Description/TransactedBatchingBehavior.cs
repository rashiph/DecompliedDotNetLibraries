namespace System.ServiceModel.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class TransactedBatchingBehavior : IEndpointBehavior
    {
        private int maxBatchSize;

        public TransactedBatchingBehavior(int maxBatchSize)
        {
            if (maxBatchSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("maxBatchSize", maxBatchSize, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            this.maxBatchSize = maxBatchSize;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (serviceEndpoint.Contract.SessionMode == SessionMode.Required)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoBatchingForSession")));
            }
            behavior.CallbackDispatchRuntime.ChannelDispatcher.MaxTransactedBatchSize = this.MaxBatchSize;
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            if (endpointDispatcher.DispatchRuntime.ReleaseServiceInstanceOnTransactionComplete)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoBatchingForReleaseOnComplete")));
            }
            if (serviceEndpoint.Contract.SessionMode == SessionMode.Required)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoBatchingForSession")));
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            BindingElementCollection elements = serviceEndpoint.Binding.CreateBindingElements();
            bool flag = false;
            foreach (BindingElement element in elements)
            {
                ITransactedBindingElement element2 = element as ITransactedBindingElement;
                if ((element2 != null) && element2.TransactedReceiveEnabled)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxTransactedBindingNeeded")));
            }
        }

        public int MaxBatchSize
        {
            get
            {
                return this.maxBatchSize;
            }
            set
            {
                if (value < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.maxBatchSize = value;
            }
        }
    }
}

