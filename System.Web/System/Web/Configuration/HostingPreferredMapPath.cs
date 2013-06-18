namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web.Hosting;

    internal class HostingPreferredMapPath : IConfigMapPath
    {
        private IConfigMapPath _hostingConfigMapPath;
        private IConfigMapPath _iisConfigMapPath;

        private HostingPreferredMapPath(IConfigMapPath iisConfigMapPath, IConfigMapPath hostingConfigMapPath)
        {
            this._iisConfigMapPath = iisConfigMapPath;
            this._hostingConfigMapPath = hostingConfigMapPath;
        }

        public string GetAppPathForPath(string siteID, string path)
        {
            string appPathForPath = this._hostingConfigMapPath.GetAppPathForPath(siteID, path);
            if (appPathForPath == null)
            {
                appPathForPath = this._iisConfigMapPath.GetAppPathForPath(siteID, path);
            }
            return appPathForPath;
        }

        public void GetDefaultSiteNameAndID(out string siteName, out string siteID)
        {
            this._hostingConfigMapPath.GetDefaultSiteNameAndID(out siteName, out siteID);
            if (string.IsNullOrEmpty(siteID))
            {
                this._iisConfigMapPath.GetDefaultSiteNameAndID(out siteName, out siteID);
            }
        }

        internal static IConfigMapPath GetInstance()
        {
            IConfigMapPath instance = IISMapPath.GetInstance();
            IConfigMapPath configMapPath = HostingEnvironment.ConfigMapPath;
            if ((configMapPath != null) && !(instance.GetType() == configMapPath.GetType()))
            {
                return new HostingPreferredMapPath(instance, configMapPath);
            }
            return instance;
        }

        public string GetMachineConfigFilename()
        {
            string machineConfigFilename = this._hostingConfigMapPath.GetMachineConfigFilename();
            if (string.IsNullOrEmpty(machineConfigFilename))
            {
                machineConfigFilename = this._iisConfigMapPath.GetMachineConfigFilename();
            }
            return machineConfigFilename;
        }

        public void GetPathConfigFilename(string siteID, string path, out string directory, out string baseName)
        {
            this._hostingConfigMapPath.GetPathConfigFilename(siteID, path, out directory, out baseName);
            if (string.IsNullOrEmpty(directory))
            {
                this._iisConfigMapPath.GetPathConfigFilename(siteID, path, out directory, out baseName);
            }
        }

        public string GetRootWebConfigFilename()
        {
            string rootWebConfigFilename = this._hostingConfigMapPath.GetRootWebConfigFilename();
            if (string.IsNullOrEmpty(rootWebConfigFilename))
            {
                rootWebConfigFilename = this._iisConfigMapPath.GetRootWebConfigFilename();
            }
            return rootWebConfigFilename;
        }

        public string MapPath(string siteID, string path)
        {
            string str = this._hostingConfigMapPath.MapPath(siteID, path);
            if (string.IsNullOrEmpty(str))
            {
                str = this._iisConfigMapPath.MapPath(siteID, path);
            }
            return str;
        }

        public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
        {
            this._hostingConfigMapPath.ResolveSiteArgument(siteArgument, out siteName, out siteID);
            if (string.IsNullOrEmpty(siteID))
            {
                this._iisConfigMapPath.ResolveSiteArgument(siteArgument, out siteName, out siteID);
            }
        }
    }
}

