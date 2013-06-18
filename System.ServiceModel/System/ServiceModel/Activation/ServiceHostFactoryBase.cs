namespace System.ServiceModel.Activation
{
    using System;
    using System.ServiceModel;

    public abstract class ServiceHostFactoryBase
    {
        protected ServiceHostFactoryBase()
        {
        }

        public abstract ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses);
    }
}

