namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ServiceDeploymentInfo
    {
        public ServiceDeploymentInfo(string virtualPath, ServiceHostFactoryBase serviceHostFactory, string serviceType)
        {
            this.VirtualPath = virtualPath;
            this.ServiceHostFactory = serviceHostFactory;
            this.ServiceType = serviceType;
            this.MessageHandledByRoute = false;
        }

        public bool MessageHandledByRoute { get; set; }

        public ServiceHostFactoryBase ServiceHostFactory { get; private set; }

        public string ServiceType { get; private set; }

        public string VirtualPath { get; private set; }
    }
}

