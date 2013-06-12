namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    internal interface IConfigMapPath2
    {
        VirtualPath GetAppPathForPath(string siteID, VirtualPath path);
        void GetPathConfigFilename(string siteID, VirtualPath path, out string directory, out string baseName);
        string MapPath(string siteID, VirtualPath path);
    }
}

