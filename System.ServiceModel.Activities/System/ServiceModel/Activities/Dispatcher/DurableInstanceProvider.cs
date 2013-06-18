namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    internal class DurableInstanceProvider : IInstanceProvider
    {
        private ServiceHostBase serviceHost;
        private object singletonDurableInstance;

        public DurableInstanceProvider(ServiceHostBase serviceHost)
        {
            this.serviceHost = serviceHost;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.Instance;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.Instance;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }

        private object Instance
        {
            get
            {
                if (this.singletonDurableInstance == null)
                {
                    this.singletonDurableInstance = new object();
                }
                return this.singletonDurableInstance;
            }
        }
    }
}

