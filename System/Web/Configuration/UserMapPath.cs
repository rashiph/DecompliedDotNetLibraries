namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Util;

    public class UserMapPath : IConfigMapPath
    {
        private string _machineConfigFilename;
        private bool _pathsAreLocal;
        private string _rootWebConfigFilename;
        private string _siteID;
        private string _siteName;
        private WebConfigurationFileMap _webFileMap;

        public UserMapPath(ConfigurationFileMap fileMap) : this(fileMap, true)
        {
        }

        internal UserMapPath(ConfigurationFileMap fileMap, bool pathsAreLocal)
        {
            this._pathsAreLocal = pathsAreLocal;
            if (!string.IsNullOrEmpty(fileMap.MachineConfigFilename))
            {
                if (this._pathsAreLocal)
                {
                    this._machineConfigFilename = Path.GetFullPath(fileMap.MachineConfigFilename);
                }
                else
                {
                    this._machineConfigFilename = fileMap.MachineConfigFilename;
                }
            }
            if (string.IsNullOrEmpty(this._machineConfigFilename))
            {
                this._machineConfigFilename = HttpConfigurationSystem.MachineConfigurationFilePath;
                this._rootWebConfigFilename = HttpConfigurationSystem.RootWebConfigurationFilePath;
            }
            else
            {
                this._rootWebConfigFilename = Path.Combine(Path.GetDirectoryName(this._machineConfigFilename), "web.config");
            }
            this._webFileMap = fileMap as WebConfigurationFileMap;
            if (this._webFileMap != null)
            {
                if (!string.IsNullOrEmpty(this._webFileMap.Site))
                {
                    this._siteName = this._webFileMap.Site;
                    this._siteID = this._webFileMap.Site;
                }
                else
                {
                    this._siteName = WebConfigurationHost.DefaultSiteName;
                    this._siteID = "1";
                }
                if (this._pathsAreLocal)
                {
                    foreach (string str in this._webFileMap.VirtualDirectories)
                    {
                        this._webFileMap.VirtualDirectories[str].Validate();
                    }
                }
                VirtualDirectoryMapping mapping2 = this._webFileMap.VirtualDirectories[null];
                if (mapping2 != null)
                {
                    this._rootWebConfigFilename = Path.Combine(mapping2.PhysicalDirectory, mapping2.ConfigFileBaseName);
                    this._webFileMap.VirtualDirectories.Remove(null);
                }
            }
        }

        public string GetAppPathForPath(string siteID, string path)
        {
            VirtualPath appPathForPath = this.GetAppPathForPath(siteID, VirtualPath.Create(path));
            if (appPathForPath == null)
            {
                return null;
            }
            return appPathForPath.VirtualPathString;
        }

        private VirtualPath GetAppPathForPath(string siteID, VirtualPath path)
        {
            if (!this.IsSiteMatch(siteID))
            {
                return null;
            }
            VirtualDirectoryMapping pathMapping = this.GetPathMapping(path, true);
            if (pathMapping == null)
            {
                return null;
            }
            return pathMapping.VirtualDirectoryObject;
        }

        public void GetDefaultSiteNameAndID(out string siteName, out string siteID)
        {
            siteName = this._siteName;
            siteID = this._siteID;
        }

        public string GetMachineConfigFilename()
        {
            return this._machineConfigFilename;
        }

        public void GetPathConfigFilename(string siteID, string path, out string directory, out string baseName)
        {
            this.GetPathConfigFilename(siteID, VirtualPath.Create(path), out directory, out baseName);
        }

        private void GetPathConfigFilename(string siteID, VirtualPath path, out string directory, out string baseName)
        {
            directory = null;
            baseName = null;
            if (this.IsSiteMatch(siteID))
            {
                VirtualDirectoryMapping pathMapping = this.GetPathMapping(path, false);
                if (pathMapping != null)
                {
                    directory = this.GetPhysicalPathForPath(path.VirtualPathString, pathMapping);
                    if (directory != null)
                    {
                        baseName = pathMapping.ConfigFileBaseName;
                    }
                }
            }
        }

        private VirtualDirectoryMapping GetPathMapping(VirtualPath path, bool onlyApps)
        {
            if (this._webFileMap == null)
            {
                return null;
            }
            string virtualPathStringNoTrailingSlash = path.VirtualPathStringNoTrailingSlash;
            while (true)
            {
                VirtualDirectoryMapping mapping = this._webFileMap.VirtualDirectories[virtualPathStringNoTrailingSlash];
                if ((mapping != null) && (!onlyApps || mapping.IsAppRoot))
                {
                    return mapping;
                }
                if (virtualPathStringNoTrailingSlash == "/")
                {
                    return null;
                }
                int length = virtualPathStringNoTrailingSlash.LastIndexOf('/');
                if (length == 0)
                {
                    virtualPathStringNoTrailingSlash = "/";
                }
                else
                {
                    virtualPathStringNoTrailingSlash = virtualPathStringNoTrailingSlash.Substring(0, length);
                }
            }
        }

        private string GetPhysicalPathForPath(string path, VirtualDirectoryMapping mapping)
        {
            string physicalDirectory;
            int length = mapping.VirtualDirectory.Length;
            if (path.Length == length)
            {
                physicalDirectory = mapping.PhysicalDirectory;
            }
            else
            {
                string str2;
                if (path[length] == '/')
                {
                    str2 = path.Substring(length + 1);
                }
                else
                {
                    str2 = path.Substring(length);
                }
                str2 = str2.Replace('/', '\\');
                physicalDirectory = Path.Combine(mapping.PhysicalDirectory, str2);
            }
            if (this._pathsAreLocal && System.Web.Util.FileUtil.IsSuspiciousPhysicalPath(physicalDirectory))
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_map_path", new object[] { path }));
            }
            return physicalDirectory;
        }

        public string GetRootWebConfigFilename()
        {
            return this._rootWebConfigFilename;
        }

        private bool IsSiteMatch(string site)
        {
            if (!string.IsNullOrEmpty(site) && !System.Web.Util.StringUtil.EqualsIgnoreCase(site, this._siteName))
            {
                return System.Web.Util.StringUtil.EqualsIgnoreCase(site, this._siteID);
            }
            return true;
        }

        public string MapPath(string siteID, string path)
        {
            return this.MapPath(siteID, VirtualPath.Create(path));
        }

        private string MapPath(string siteID, VirtualPath path)
        {
            string str;
            string str2;
            this.GetPathConfigFilename(siteID, path, out str, out str2);
            return str;
        }

        public void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
        {
            if (this.IsSiteMatch(siteArgument))
            {
                siteName = this._siteName;
                siteID = this._siteID;
            }
            else
            {
                siteName = siteArgument;
                siteID = null;
            }
        }
    }
}

