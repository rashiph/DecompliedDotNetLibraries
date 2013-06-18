namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Hosting;

    internal interface IServerConfig
    {
        string GetSiteNameFromSiteID(string siteID);
        bool GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password);
        string[] GetVirtualSubdirs(VirtualPath path, bool inApp);
        long GetW3WPMemoryLimitInKB();
        string MapPath(IApplicationHost appHost, VirtualPath path);
    }
}

