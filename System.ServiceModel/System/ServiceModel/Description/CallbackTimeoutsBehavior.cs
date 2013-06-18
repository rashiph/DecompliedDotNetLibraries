namespace System.ServiceModel.Description
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class CallbackTimeoutsBehavior : IEndpointBehavior
    {
        private TimeSpan transactionTimeout = TimeSpan.Zero;

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
            if (this.transactionTimeout != TimeSpan.Zero)
            {
                ChannelDispatcher channelDispatcher = behavior.CallbackDispatchRuntime.ChannelDispatcher;
                if (((channelDispatcher != null) && (channelDispatcher.TransactionTimeout == TimeSpan.Zero)) || (channelDispatcher.TransactionTimeout > this.transactionTimeout))
                {
                    channelDispatcher.TransactionTimeout = this.transactionTimeout;
                }
            }
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFXEndpointBehaviorUsedOnWrongSide", new object[] { typeof(CallbackTimeoutsBehavior).Name })));
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
        }

        public TimeSpan TransactionTimeout
        {
            get
            {
                return this.transactionTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                this.transactionTimeout = value;
            }
        }
    }
}

