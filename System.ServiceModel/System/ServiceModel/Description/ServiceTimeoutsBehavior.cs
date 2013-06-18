namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class ServiceTimeoutsBehavior : IServiceBehavior
    {
        private TimeSpan transactionTimeout = TimeSpan.Zero;

        internal ServiceTimeoutsBehavior(TimeSpan transactionTimeout)
        {
            this.transactionTimeout = transactionTimeout;
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (this.transactionTimeout != TimeSpan.Zero)
            {
                for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher dispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                    if (((dispatcher != null) && dispatcher.HasApplicationEndpoints()) && ((dispatcher.TransactionTimeout == TimeSpan.Zero) || (dispatcher.TransactionTimeout > this.transactionTimeout)))
                    {
                        dispatcher.TransactionTimeout = this.transactionTimeout;
                    }
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        internal TimeSpan TransactionTimeout
        {
            get
            {
                return this.transactionTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
                }
                this.transactionTimeout = value;
            }
        }
    }
}

