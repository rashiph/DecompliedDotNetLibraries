namespace System.Web.Hosting
{
    using System;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal class SimpleApplicationHost : MarshalByRefObject, IApplicationHost
    {
        private string _appPhysicalPath;
        private VirtualPath _appVirtualPath;

        internal SimpleApplicationHost(VirtualPath virtualPath, string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("physicalPath");
            }
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath))
            {
                throw ExceptionUtil.ParameterInvalid(physicalPath);
            }
            this._appVirtualPath = virtualPath;
            this._appPhysicalPath = StringUtil.StringEndsWith(physicalPath, @"\") ? physicalPath : (physicalPath + @"\");
        }

        public string GetVirtualPath()
        {
            return this._appVirtualPath.VirtualPathString;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void MessageReceived()
        {
        }

        IConfigMapPathFactory IApplicationHost.GetConfigMapPathFactory()
        {
            return new SimpleConfigMapPathFactory();
        }

        IntPtr IApplicationHost.GetConfigToken()
        {
            return IntPtr.Zero;
        }

        string IApplicationHost.GetPhysicalPath()
        {
            return this._appPhysicalPath;
        }

        string IApplicationHost.GetSiteID()
        {
            return "1";
        }

        string IApplicationHost.GetSiteName()
        {
            return WebConfigurationHost.DefaultSiteName;
        }
    }
}

