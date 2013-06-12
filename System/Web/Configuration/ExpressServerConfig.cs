namespace System.Web.Configuration
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class ExpressServerConfig : IServerConfig, IServerConfig2, IConfigMapPath, IConfigMapPath2, IDisposable
    {
        private string _currentAppSiteName;
        private NativeConfig _nativeConfig;
        private static object s_initLock = new object();
        private static ExpressServerConfig s_instance;

        static ExpressServerConfig()
        {
            HttpRuntime.ForceStaticInit();
        }

        private ExpressServerConfig()
        {
        }

        internal ExpressServerConfig(string version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            this._nativeConfig = new NativeConfig(version);
        }

        private VirtualPath GetAppPathForPathWorker(string siteID, VirtualPath path)
        {
            string str;
            uint result = 0;
            if (!uint.TryParse(siteID, out result))
            {
                return VirtualPath.RootVirtualPath;
            }
            IntPtr zero = IntPtr.Zero;
            int cchPath = 0;
            try
            {
                str = ((this._nativeConfig.MgdGetAppPathForPath(result, path.VirtualPathString, out zero, out cchPath) == 0) && (cchPath > 0)) ? StringUtil.StringFromWCharPtr(zero, cchPath) : null;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(zero);
                }
            }
            if (str == null)
            {
                return VirtualPath.RootVirtualPath;
            }
            return VirtualPath.Create(str);
        }

        internal static IServerConfig GetInstance(string version)
        {
            if (s_instance == null)
            {
                lock (s_initLock)
                {
                    if (s_instance == null)
                    {
                        if (Thread.GetDomain().IsDefaultAppDomain())
                        {
                            throw new InvalidOperationException();
                        }
                        s_instance = new ExpressServerConfig(version);
                    }
                }
            }
            return s_instance;
        }

        private void GetPathConfigFilenameWorker(string siteID, VirtualPath path, out string directory, out string baseName)
        {
            directory = this.MapPathCaching(siteID, path);
            if (directory != null)
            {
                baseName = "web.config";
            }
            else
            {
                baseName = null;
            }
        }

        private string MapPathCaching(string siteID, VirtualPath path)
        {
            string originalResult = this._nativeConfig.MapPathDirect(((IServerConfig) this).GetSiteNameFromSiteID(siteID), path);
            if (((originalResult != null) && (originalResult.Length == 2)) && (originalResult[1] == ':'))
            {
                originalResult = originalResult + @"\";
            }
            if (HttpRuntime.IsMapPathRelaxed)
            {
                originalResult = HttpRuntime.GetRelaxedMapPathResult(originalResult);
            }
            if (!FileUtil.IsSuspiciousPhysicalPath(originalResult))
            {
                return originalResult;
            }
            if (!HttpRuntime.IsMapPathRelaxed)
            {
                throw new HttpException(System.Web.SR.GetString("Cannot_map_path", new object[] { path }));
            }
            return HttpRuntime.GetRelaxedMapPathResult(null);
        }

        private string MapPathWorker(string siteID, VirtualPath path)
        {
            return this.MapPathCaching(siteID, path);
        }

        void IDisposable.Dispose()
        {
            NativeConfig config = this._nativeConfig;
            this._nativeConfig = null;
            if (config != null)
            {
                config.Dispose();
            }
        }

        string IConfigMapPath.GetAppPathForPath(string siteID, string path)
        {
            return this.GetAppPathForPathWorker(siteID, VirtualPath.Create(path)).VirtualPathString;
        }

        void IConfigMapPath.GetDefaultSiteNameAndID(out string siteName, out string siteID)
        {
            siteID = "1";
            siteName = this._nativeConfig.GetSiteNameFromId(1);
        }

        string IConfigMapPath.GetMachineConfigFilename()
        {
            return HttpConfigurationSystem.MachineConfigurationFilePath;
        }

        void IConfigMapPath.GetPathConfigFilename(string siteID, string path, out string directory, out string baseName)
        {
            this.GetPathConfigFilenameWorker(siteID, VirtualPath.Create(path), out directory, out baseName);
        }

        string IConfigMapPath.GetRootWebConfigFilename()
        {
            return HttpConfigurationSystem.RootWebConfigurationFilePath;
        }

        string IConfigMapPath.MapPath(string siteID, string path)
        {
            return this.MapPathWorker(siteID, VirtualPath.Create(path));
        }

        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
        {
            if ((string.IsNullOrEmpty(siteArgument) || StringUtil.EqualsIgnoreCase(siteArgument, "1")) || StringUtil.EqualsIgnoreCase(siteArgument, this._nativeConfig.GetSiteNameFromId(1)))
            {
                siteName = this._nativeConfig.GetSiteNameFromId(1);
                siteID = "1";
            }
            else
            {
                siteName = string.Empty;
                siteID = string.Empty;
                string siteNameFromId = null;
                if (IISMapPath.IsSiteId(siteArgument))
                {
                    uint num;
                    if (uint.TryParse(siteArgument, out num))
                    {
                        siteNameFromId = this._nativeConfig.GetSiteNameFromId(num);
                    }
                }
                else
                {
                    uint num2 = this._nativeConfig.MgdResolveSiteName(siteArgument);
                    if (num2 != 0)
                    {
                        siteID = num2.ToString(CultureInfo.InvariantCulture);
                        siteName = siteArgument;
                        return;
                    }
                }
                if (!string.IsNullOrEmpty(siteNameFromId))
                {
                    siteName = siteNameFromId;
                    siteID = siteArgument;
                }
                else
                {
                    siteName = siteArgument;
                    siteID = string.Empty;
                }
            }
        }

        VirtualPath IConfigMapPath2.GetAppPathForPath(string siteID, VirtualPath path)
        {
            return this.GetAppPathForPathWorker(siteID, path);
        }

        void IConfigMapPath2.GetPathConfigFilename(string siteID, VirtualPath path, out string directory, out string baseName)
        {
            this.GetPathConfigFilenameWorker(siteID, path, out directory, out baseName);
        }

        string IConfigMapPath2.MapPath(string siteID, VirtualPath path)
        {
            return this.MapPathWorker(siteID, path);
        }

        string IServerConfig.GetSiteNameFromSiteID(string siteID)
        {
            uint num;
            if (!uint.TryParse(siteID, out num))
            {
                return string.Empty;
            }
            return this._nativeConfig.GetSiteNameFromId(num);
        }

        bool IServerConfig.GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password)
        {
            bool flag = false;
            username = null;
            password = null;
            IntPtr zero = IntPtr.Zero;
            int cchUserName = 0;
            IntPtr bstrPassword = IntPtr.Zero;
            int cchPassword = 0;
            try
            {
                if (this._nativeConfig.MgdGetVrPathCreds(appHost.GetSiteName(), path.VirtualPathString, out zero, out cchUserName, out bstrPassword, out cchPassword) == 0)
                {
                    username = (cchUserName > 0) ? StringUtil.StringFromWCharPtr(zero, cchUserName) : null;
                    password = (cchPassword > 0) ? StringUtil.StringFromWCharPtr(bstrPassword, cchPassword) : null;
                    flag = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(zero);
                }
                if (bstrPassword != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstrPassword);
                }
            }
            return flag;
        }

        string[] IServerConfig.GetVirtualSubdirs(VirtualPath path, bool inApp)
        {
            if (!inApp)
            {
                throw new NotSupportedException();
            }
            string virtualPathString = path.VirtualPathString;
            string[] strArray = null;
            int num = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr pBstr = IntPtr.Zero;
            int cBstr = 0;
            try
            {
                int count = 0;
                int num4 = this._nativeConfig.MgdGetAppCollection(this.CurrentAppSiteName, virtualPathString, out pBstr, out cBstr, out zero, out count);
                if ((num4 < 0) || (pBstr == IntPtr.Zero))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cant_Enumerate_NativeDirs", new object[] { num4 }));
                }
                string str2 = StringUtil.StringFromWCharPtr(pBstr, cBstr);
                Marshal.FreeBSTR(pBstr);
                pBstr = IntPtr.Zero;
                cBstr = 0;
                strArray = new string[count];
                int length = virtualPathString.Length;
                if (virtualPathString[length - 1] == '/')
                {
                    length--;
                }
                int startIndex = str2.Length;
                string str3 = (length > startIndex) ? virtualPathString.Substring(startIndex, length - startIndex) : string.Empty;
                for (uint i = 0; i < count; i++)
                {
                    num4 = UnsafeIISMethods.MgdGetNextVPath(zero, i, out pBstr, out cBstr);
                    if ((num4 < 0) || (pBstr == IntPtr.Zero))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("Cant_Enumerate_NativeDirs", new object[] { num4 }));
                    }
                    string str4 = (cBstr > 1) ? StringUtil.StringFromWCharPtr(pBstr, cBstr) : null;
                    Marshal.FreeBSTR(pBstr);
                    pBstr = IntPtr.Zero;
                    cBstr = 0;
                    if ((str4 != null) && (str4.Length > str3.Length))
                    {
                        if (str3.Length == 0)
                        {
                            if (str4.IndexOf('/', 1) == -1)
                            {
                                strArray[num++] = str4.Substring(1);
                            }
                        }
                        else if (StringUtil.EqualsIgnoreCase(str3, 0, str4, 0, str3.Length))
                        {
                            int index = str4.IndexOf('/', 1 + str3.Length);
                            if (index > -1)
                            {
                                strArray[num++] = str4.Substring(str3.Length + 1, index - str3.Length);
                            }
                            else
                            {
                                strArray[num++] = str4.Substring(str3.Length + 1);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.Release(zero);
                    zero = IntPtr.Zero;
                }
                if (pBstr != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(pBstr);
                    pBstr = IntPtr.Zero;
                }
            }
            string[] strArray2 = null;
            if (num > 0)
            {
                strArray2 = new string[num];
                for (int j = 0; j < strArray2.Length; j++)
                {
                    strArray2[j] = strArray[j];
                }
            }
            return strArray2;
        }

        long IServerConfig.GetW3WPMemoryLimitInKB()
        {
            long limit = 0L;
            if (UnsafeIISMethods.MgdGetMemoryLimitKB(out limit) < 0)
            {
                return 0L;
            }
            return limit;
        }

        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path)
        {
            string siteName = (appHost == null) ? this.CurrentAppSiteName : appHost.GetSiteName();
            string physicalPath = this._nativeConfig.MapPathDirect(siteName, path);
            if (FileUtil.IsSuspiciousPhysicalPath(physicalPath))
            {
                throw new InvalidOperationException(System.Web.SR.GetString("Cannot_map_path", new object[] { path.VirtualPathString }));
            }
            return physicalPath;
        }

        bool IServerConfig2.IsWithinApp(string virtualPath)
        {
            return this._nativeConfig.MgdIsWithinApp(this.CurrentAppSiteName, HttpRuntime.AppDomainAppVirtualPathString, virtualPath);
        }

        private string CurrentAppSiteName
        {
            get
            {
                string siteNameNoDemand = this._currentAppSiteName;
                if (siteNameNoDemand == null)
                {
                    siteNameNoDemand = HostingEnvironment.SiteNameNoDemand;
                    if (siteNameNoDemand == null)
                    {
                        siteNameNoDemand = this._nativeConfig.GetSiteNameFromId(1);
                    }
                    this._currentAppSiteName = siteNameNoDemand;
                }
                return siteNameNoDemand;
            }
        }
    }
}

