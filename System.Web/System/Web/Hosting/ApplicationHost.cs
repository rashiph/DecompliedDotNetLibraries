namespace System.Web.Hosting
{
    using System;
    using System.IO;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public sealed class ApplicationHost
    {
        private ApplicationHost()
        {
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static object CreateApplicationHost(Type hostType, string virtualDir, string physicalDir)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
            }
            if (!StringUtil.StringEndsWith(physicalDir, Path.DirectorySeparatorChar))
            {
                physicalDir = physicalDir + Path.DirectorySeparatorChar;
            }
            ApplicationManager applicationManager = ApplicationManager.GetApplicationManager();
            string appId = (virtualDir + physicalDir).GetHashCode().ToString("x");
            return applicationManager.CreateInstanceInNewWorkerAppDomain(hostType, appId, VirtualPath.CreateNonRelative(virtualDir), physicalDir).Unwrap();
        }
    }
}

