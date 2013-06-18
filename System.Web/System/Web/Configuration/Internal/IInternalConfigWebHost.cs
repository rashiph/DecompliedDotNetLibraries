namespace System.Web.Configuration.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public interface IInternalConfigWebHost
    {
        string GetConfigPathFromSiteIDAndVPath(string siteID, string vpath);
        void GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out string vpath);
    }
}

