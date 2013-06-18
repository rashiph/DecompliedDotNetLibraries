namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;

    public sealed class VirtualPathExtension : IExtension<ServiceHostBase>
    {
        internal VirtualPathExtension(string virtualPath, string applicationVirtualPath, string siteName)
        {
            this.VirtualPath = virtualPath;
            this.ApplicationVirtualPath = applicationVirtualPath;
            this.SiteName = siteName;
        }

        public void Attach(ServiceHostBase owner)
        {
        }

        public void Detach(ServiceHostBase owner)
        {
            throw System.ServiceModel.Diagnostics.Application.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.SR.GetString("Hosting_VirtualPathExtenstionCanNotBeDetached")));
        }

        public string ApplicationVirtualPath { get; private set; }

        public string SiteName { get; private set; }

        public string VirtualPath { get; private set; }
    }
}

