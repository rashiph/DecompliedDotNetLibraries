namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public class ServiceThrottlingBehavior : IServiceBehavior
    {
        private int calls = ServiceThrottle.DefaultMaxConcurrentCallsCpuCount;
        internal static int DefaultMaxConcurrentInstances = (ServiceThrottle.DefaultMaxConcurrentCallsCpuCount + ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount);
        private int instances = 0x7fffffff;
        private bool maxInstanceSetExplicitly;
        private int sessions = ServiceThrottle.DefaultMaxConcurrentSessionsCpuCount;

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));
            }
            ServiceThrottle serviceThrottle = serviceHostBase.ServiceThrottle;
            serviceThrottle.MaxConcurrentCalls = this.calls;
            serviceThrottle.MaxConcurrentSessions = this.sessions;
            serviceThrottle.MaxConcurrentInstances = this.MaxConcurrentInstances;
            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher dispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (dispatcher != null)
                {
                    dispatcher.ServiceThrottle = serviceThrottle;
                }
            }
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public int MaxConcurrentCalls
        {
            get
            {
                return this.calls;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxThrottleLimitMustBeGreaterThanZero0")));
                }
                this.calls = value;
            }
        }

        public int MaxConcurrentInstances
        {
            get
            {
                if (!this.maxInstanceSetExplicitly)
                {
                    this.instances = this.calls + this.sessions;
                    if (this.instances < 0)
                    {
                        this.instances = 0x7fffffff;
                    }
                }
                return this.instances;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxThrottleLimitMustBeGreaterThanZero0")));
                }
                this.instances = value;
                this.maxInstanceSetExplicitly = true;
            }
        }

        public int MaxConcurrentSessions
        {
            get
            {
                return this.sessions;
            }
            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxThrottleLimitMustBeGreaterThanZero0")));
                }
                this.sessions = value;
            }
        }
    }
}

