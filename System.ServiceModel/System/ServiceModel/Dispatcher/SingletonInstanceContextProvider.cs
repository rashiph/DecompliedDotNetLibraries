namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal class SingletonInstanceContextProvider : InstanceContextProviderBase
    {
        private InstanceContext singleton;
        private object thisLock;

        internal SingletonInstanceContextProvider(DispatchRuntime dispatchRuntime) : base(dispatchRuntime)
        {
            this.thisLock = new object();
        }

        public override InstanceContext GetExistingInstanceContext(Message message, IContextChannel channel)
        {
            ServiceChannel serviceChannelFromProxy = base.GetServiceChannelFromProxy(channel);
            if ((serviceChannelFromProxy != null) && serviceChannelFromProxy.HasSession)
            {
                this.SingletonInstance.BindIncomingChannel(serviceChannelFromProxy);
            }
            return this.SingletonInstance;
        }

        public override void InitializeInstanceContext(InstanceContext instanceContext, Message message, IContextChannel channel)
        {
        }

        public override bool IsIdle(InstanceContext instanceContext)
        {
            return false;
        }

        public override void NotifyIdle(InstanceContextIdleCallback callback, InstanceContext instanceContext)
        {
        }

        internal InstanceContext SingletonInstance
        {
            get
            {
                if (this.singleton == null)
                {
                    lock (this.thisLock)
                    {
                        if (this.singleton == null)
                        {
                            InstanceContext singletonInstanceContext = base.DispatchRuntime.SingletonInstanceContext;
                            if (singletonInstanceContext == null)
                            {
                                singletonInstanceContext = new InstanceContext(base.DispatchRuntime.ChannelDispatcher.Host, false);
                            }
                            if (singletonInstanceContext.State == CommunicationState.Created)
                            {
                                singletonInstanceContext.Open();
                            }
                            singletonInstanceContext.IsUserCreated = false;
                            this.singleton = singletonInstanceContext;
                        }
                    }
                }
                return this.singleton;
            }
        }
    }
}

