namespace System.Web.Configuration
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class MetabaseServerConfig : IServerConfig, IConfigMapPath, IConfigMapPath2
    {
        private string _defaultSiteName;
        private object _mapPathCacheLock;
        private string _siteIdForCurrentApplication;
        private const int BUFSIZE = 0x105;
        private const string DEFAULT_ROOTAPPID = "/LM/W3SVC/1/ROOT";
        private const string DEFAULT_SITEID = "1";
        private const string LMW3SVC_PREFIX = "/LM/W3SVC/";
        private const int MAX_PATH = 260;
        private const string ROOT_SUFFIX = "/ROOT";
        private static object s_initLock = new object();
        private static MetabaseServerConfig s_instance;

        private MetabaseServerConfig()
        {
            HttpRuntime.ForceStaticInit();
            this._mapPathCacheLock = new object();
            this.MBGetSiteNameFromSiteID("1", out this._defaultSiteName);
            this._siteIdForCurrentApplication = HostingEnvironment.SiteID;
            if (this._siteIdForCurrentApplication == null)
            {
                this._siteIdForCurrentApplication = "1";
            }
        }

        private string FixupPathSlash(string path)
        {
            if (path == null)
            {
                return null;
            }
            int length = path.Length;
            if ((length != 0) && (path[length - 1] == '/'))
            {
                return path.Substring(0, length - 1);
            }
            return path;
        }

        private string GetAboPath(string siteID, string path)
        {
            return (this.GetRootAppIDFromSiteID(siteID) + this.FixupPathSlash(path));
        }

        private VirtualPath GetAppPathForPathWorker(string siteID, VirtualPath vpath)
        {
            string aboPath = this.GetAboPath(siteID, vpath.VirtualPathString);
            string str2 = this.MBGetAppPath(aboPath);
            if (str2 == null)
            {
                return VirtualPath.RootVirtualPath;
            }
            string rootAppIDFromSiteID = this.GetRootAppIDFromSiteID(siteID);
            if (StringUtil.EqualsIgnoreCase(rootAppIDFromSiteID, str2))
            {
                return VirtualPath.RootVirtualPath;
            }
            return VirtualPath.CreateAbsolute(str2.Substring(rootAppIDFromSiteID.Length));
        }

        internal static IServerConfig GetInstance()
        {
            if (s_instance == null)
            {
                lock (s_initLock)
                {
                    if (s_instance == null)
                    {
                        s_instance = new MetabaseServerConfig();
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

        private string GetRootAppIDFromSiteID(string siteId)
        {
            return ("/LM/W3SVC/" + siteId + "/ROOT");
        }

        private string MapPathActual(string siteID, VirtualPath path)
        {
            string rootAppIDFromSiteID = this.GetRootAppIDFromSiteID(siteID);
            return this.MBMapPath(rootAppIDFromSiteID, path.VirtualPathString);
        }

        private string MapPathCaching(string siteID, VirtualPath path)
        {
            VirtualPath path2 = path;
            string key = "f" + siteID + path.VirtualPathString;
            MapPathCacheInfo info = (MapPathCacheInfo) HttpRuntime.CacheInternal.Get(key);
            if (info == null)
            {
                lock (this._mapPathCacheLock)
                {
                    info = (MapPathCacheInfo) HttpRuntime.CacheInternal.Get(key);
                    if (info == null)
                    {
                        info = new MapPathCacheInfo();
                        HttpRuntime.CacheInternal.UtcInsert(key, info, null, DateTime.UtcNow.AddMinutes(10.0), Cache.NoSlidingExpiration);
                    }
                }
            }
            if (!info.Evaluated)
            {
                lock (info)
                {
                    if ((!info.Evaluated && HttpRuntime.IsMapPathRelaxed) && (path.VirtualPathString.Length > 1))
                    {
                        VirtualPath parent = path.Parent;
                        if (parent != null)
                        {
                            string virtualPathString = parent.VirtualPathString;
                            if ((virtualPathString.Length > 1) && StringUtil.StringEndsWith(virtualPathString, '/'))
                            {
                                parent = VirtualPath.Create(virtualPathString.Substring(0, virtualPathString.Length - 1));
                            }
                            try
                            {
                                string str3 = this.MapPathCaching(siteID, parent);
                                if (str3 == HttpRuntime.GetRelaxedMapPathResult(null))
                                {
                                    info.MapPathResult = str3;
                                    info.Evaluated = true;
                                }
                            }
                            catch
                            {
                                info.MapPathResult = HttpRuntime.GetRelaxedMapPathResult(null);
                                info.Evaluated = true;
                            }
                        }
                    }
                    if (!info.Evaluated)
                    {
                        string originalResult = null;
                        try
                        {
                            originalResult = this.MapPathActual(siteID, path);
                            if (HttpRuntime.IsMapPathRelaxed)
                            {
                                originalResult = HttpRuntime.GetRelaxedMapPathResult(originalResult);
                            }
                            if (FileUtil.IsSuspiciousPhysicalPath(originalResult))
                            {
                                if (!HttpRuntime.IsMapPathRelaxed)
                                {
                                    throw new HttpException(System.Web.SR.GetString("Cannot_map_path", new object[] { path }));
                                }
                                originalResult = HttpRuntime.GetRelaxedMapPathResult(null);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (!HttpRuntime.IsMapPathRelaxed)
                            {
                                info.CachedException = exception;
                                info.Evaluated = true;
                                throw;
                            }
                            originalResult = HttpRuntime.GetRelaxedMapPathResult(null);
                        }
                        if (originalResult != null)
                        {
                            info.MapPathResult = originalResult;
                            info.Evaluated = true;
                        }
                    }
                }
            }
            if (info.CachedException != null)
            {
                throw info.CachedException;
            }
            return this.MatchResult(path2, info.MapPathResult);
        }

        private string MatchResult(VirtualPath path, string result)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace('/', '\\');
                if (path.HasTrailingSlash)
                {
                    if (!UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
                    {
                        result = result + @"\";
                    }
                    return result;
                }
                if (UrlPath.PathEndsWithExtraSlash(result) && !UrlPath.PathIsDriveRoot(result))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        private string MBGetAppPath(string aboPath)
        {
            StringBuilder buffer = new StringBuilder(aboPath.Length + 1);
            if (UnsafeNativeMethods.IsapiAppHostGetAppPath(aboPath, buffer, buffer.Capacity) == 1)
            {
                return buffer.ToString();
            }
            return null;
        }

        private bool MBGetSiteIDFromSiteName(string siteName, out string siteID)
        {
            StringBuilder buffer = new StringBuilder(0x105);
            if (UnsafeNativeMethods.IsapiAppHostGetSiteId(siteName, buffer, buffer.Capacity) == 1)
            {
                siteID = buffer.ToString();
                return true;
            }
            siteID = string.Empty;
            return false;
        }

        private bool MBGetSiteNameFromSiteID(string siteID, out string siteName)
        {
            string rootAppIDFromSiteID = this.GetRootAppIDFromSiteID(siteID);
            StringBuilder buffer = new StringBuilder(0x105);
            if (UnsafeNativeMethods.IsapiAppHostGetSiteName(rootAppIDFromSiteID, buffer, buffer.Capacity) == 1)
            {
                siteName = buffer.ToString();
                return true;
            }
            siteName = string.Empty;
            return false;
        }

        private bool MBGetUncUser(string aboPath, out string username, out string password)
        {
            StringBuilder usernameBuffer = new StringBuilder(0x105);
            StringBuilder passwordBuffer = new StringBuilder(0x105);
            if (UnsafeNativeMethods.IsapiAppHostGetUncUser(aboPath, usernameBuffer, usernameBuffer.Capacity, passwordBuffer, passwordBuffer.Capacity) == 1)
            {
                username = usernameBuffer.ToString();
                password = passwordBuffer.ToString();
                return true;
            }
            username = null;
            password = null;
            return false;
        }

        private string[] MBGetVirtualSubdirs(string aboPath, bool inApp)
        {
            StringBuilder sb = new StringBuilder(0x105);
            int index = 0;
            ArrayList list = new ArrayList();
            while (true)
            {
                sb.Length = 0;
                if (UnsafeNativeMethods.IsapiAppHostGetNextVirtualSubdir(aboPath, inApp, ref index, sb, sb.Capacity) == 0)
                {
                    break;
                }
                string str = sb.ToString();
                list.Add(str);
            }
            string[] array = new string[list.Count];
            list.CopyTo(array);
            return array;
        }

        private int MBGetW3WPMemoryLimitInKB()
        {
            return UnsafeNativeMethods.GetW3WPMemoryLimitInKB();
        }

        private string MBMapPath(string appID, string path)
        {
            StringBuilder builder;
            int num2;
            int capacity = 0x105;
            while (true)
            {
                builder = new StringBuilder(capacity);
                num2 = UnsafeNativeMethods.IsapiAppHostMapPath(appID, path, builder, builder.Capacity);
                if (num2 != -2)
                {
                    break;
                }
                capacity *= 2;
            }
            switch (num2)
            {
                case -1:
                    throw new HostingEnvironmentException(System.Web.SR.GetString("Cannot_access_mappath_title"), System.Web.SR.GetString("Cannot_access_mappath_details"));

                case 1:
                    return builder.ToString();
            }
            return null;
        }

        string IConfigMapPath.GetAppPathForPath(string siteID, string vpath)
        {
            return this.GetAppPathForPathWorker(siteID, VirtualPath.Create(vpath)).VirtualPathString;
        }

        void IConfigMapPath.GetDefaultSiteNameAndID(out string siteName, out string siteID)
        {
            siteName = this._defaultSiteName;
            siteID = "1";
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

        string IConfigMapPath.MapPath(string siteID, string vpath)
        {
            return this.MapPathCaching(siteID, VirtualPath.Create(vpath));
        }

        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
        {
            if ((string.IsNullOrEmpty(siteArgument) || StringUtil.EqualsIgnoreCase(siteArgument, "1")) || StringUtil.EqualsIgnoreCase(siteArgument, this._defaultSiteName))
            {
                siteName = this._defaultSiteName;
                siteID = "1";
            }
            else
            {
                siteName = string.Empty;
                siteID = string.Empty;
                bool flag = false;
                if (IISMapPath.IsSiteId(siteArgument))
                {
                    flag = this.MBGetSiteNameFromSiteID(siteArgument, out siteName);
                }
                if (flag)
                {
                    siteID = siteArgument;
                }
                else if (this.MBGetSiteIDFromSiteName(siteArgument, out siteID))
                {
                    siteName = siteArgument;
                }
                else
                {
                    siteName = siteArgument;
                    siteID = string.Empty;
                }
            }
        }

        VirtualPath IConfigMapPath2.GetAppPathForPath(string siteID, VirtualPath vpath)
        {
            return this.GetAppPathForPathWorker(siteID, vpath);
        }

        void IConfigMapPath2.GetPathConfigFilename(string siteID, VirtualPath path, out string directory, out string baseName)
        {
            this.GetPathConfigFilenameWorker(siteID, path, out directory, out baseName);
        }

        string IConfigMapPath2.MapPath(string siteID, VirtualPath vpath)
        {
            return this.MapPathCaching(siteID, vpath);
        }

        string IServerConfig.GetSiteNameFromSiteID(string siteID)
        {
            string str;
            if (StringUtil.EqualsIgnoreCase(siteID, "1"))
            {
                return this._defaultSiteName;
            }
            this.MBGetSiteNameFromSiteID(siteID, out str);
            return str;
        }

        bool IServerConfig.GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password)
        {
            string aboPath = this.GetAboPath(appHost.GetSiteID(), path.VirtualPathString);
            return this.MBGetUncUser(aboPath, out username, out password);
        }

        string[] IServerConfig.GetVirtualSubdirs(VirtualPath path, bool inApp)
        {
            string aboPath = this.GetAboPath(this._siteIdForCurrentApplication, path.VirtualPathString);
            return this.MBGetVirtualSubdirs(aboPath, inApp);
        }

        long IServerConfig.GetW3WPMemoryLimitInKB()
        {
            return (long) this.MBGetW3WPMemoryLimitInKB();
        }

        string IServerConfig.MapPath(IApplicationHost appHost, VirtualPath path)
        {
            string siteID = (appHost == null) ? this._siteIdForCurrentApplication : appHost.GetSiteID();
            return this.MapPathCaching(siteID, path);
        }
    }
}

