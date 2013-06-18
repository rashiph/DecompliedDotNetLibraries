namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class PerSessionInstanceContextProvider : InstanceContextProviderBase
    {
        internal PerSessionInstanceContextProvider(DispatchRuntime dispatchRuntime) : base(dispatchRuntime)
        {
        }

        public override InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            ServiceChannel serviceChannelFromProxy = base.GetServiceChannelFromProxy(channel);
            if (serviceChannelFromProxy == null)
            {
                return null;
            }
            return serviceChannelFromProxy.InstanceContext;
        }

        public override void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
            ServiceChannel serviceChannelFromProxy = base.GetServiceChannelFromProxy(channel);
            if ((serviceChannelFromProxy != null) && serviceChannelFromProxy.HasSession)
            {
                instanceContext.BindIncomingChannel(serviceChannelFromProxy);
            }
        }

        public override bool IsIdle(InstanceContext instanceContext)
        {
            return true;
        }

        public override void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }
    }
}

