namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal abstract class InstanceContextProviderBase : IInstanceContextProvider
    {
        private System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime;

        internal InstanceContextProviderBase(System.ServiceModel.Dispatcher.DispatchRuntime dispatchRuntime)
        {
            this.dispatchRuntime = dispatchRuntime;
        }

        public virtual InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        internal static IInstanceContextProvider GetProviderForMode(InstanceContextMode instanceMode, System.ServiceModel.Dispatcher.DispatchRuntime runtime)
        {
            switch (instanceMode)
            {
                case InstanceContextMode.PerSession:
                    return new PerSessionInstanceContextProvider(runtime);

                case InstanceContextMode.PerCall:
                    return new PerCallInstanceContextProvider(runtime);

                case InstanceContextMode.Single:
                    return new SingletonInstanceContextProvider(runtime);
            }
            DiagnosticUtility.FailFast("InstanceContextProviderBase.GetProviderForMode: default");
            return null;
        }

        internal ServiceChannel GetServiceChannelFromProxy(IContextChannel channel)
        {
            ServiceChannel serviceChannel = channel as ServiceChannel;
            if (serviceChannel == null)
            {
                serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            }
            return serviceChannel;
        }

        public virtual void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public virtual bool IsIdle(InstanceContext instanceContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        internal static bool IsProviderPerCall(IInstanceContextProvider provider)
        {
            return (provider is PerCallInstanceContextProvider);
        }

        internal static bool IsProviderSessionful(IInstanceContextProvider provider)
        {
            return (provider is PerSessionInstanceContextProvider);
        }

        internal static bool IsProviderSingleton(IInstanceContextProvider provider)
        {
            return (provider is SingletonInstanceContextProvider);
        }

        public virtual void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public System.ServiceModel.Dispatcher.DispatchRuntime DispatchRuntime
        {
            get
            {
                return this.dispatchRuntime;
            }
        }
    }
}

