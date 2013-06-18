namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Configuration;

    internal class DllHostedComPlusServiceHost : ComPlusServiceHost
    {
        public DllHostedComPlusServiceHost(Guid clsid, ServiceElement service, ComCatalogObject applicationObject, ComCatalogObject classObject)
        {
            base.Initialize(clsid, service, applicationObject, classObject, HostingMode.ComPlus);
        }
    }
}

