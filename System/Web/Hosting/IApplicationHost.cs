namespace System.Web.Hosting
{
    using System;
    using System.Web.Configuration;

    public interface IApplicationHost
    {
        IConfigMapPathFactory GetConfigMapPathFactory();
        IntPtr GetConfigToken();
        string GetPhysicalPath();
        string GetSiteID();
        string GetSiteName();
        string GetVirtualPath();
        void MessageReceived();
    }
}

