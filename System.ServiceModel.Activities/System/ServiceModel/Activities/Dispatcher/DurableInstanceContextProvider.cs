namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class DurableInstanceContextProvider : IInstanceContextProvider
    {
        private ServiceHostBase serviceHostBase;

        public DurableInstanceContextProvider(ServiceHostBase serviceHost)
        {
            this.serviceHostBase = serviceHost;
        }

        public InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            return new InstanceContext(this.serviceHostBase);
        }

        public void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
        }

        public bool IsIdle(InstanceContext instanceContext)
        {
            return true;
        }

        public void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }
    }
}

