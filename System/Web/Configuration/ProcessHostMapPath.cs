namespace System.Web.Configuration
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.Util;

    internal sealed class ProcessHostMapPath : IConfigMapPath, IConfigMapPath2
    {
        internal static string _DefaultPhysicalPathOnMapPathFailure;
        private IProcessHostSupportFunctions _functions;
        private object _mapPathCacheLock;

        static ProcessHostMapPath()
        {
            HttpRuntime.ForceStaticInit();
        }

        internal ProcessHostMapPath(IProcessHostSupportFunctions functions)
        {
            if (functions == null)
            {
                ProcessHostConfigUtils.InitStandaloneConfig();
            }
            if (functions != null)
            {
                this._functions = Misc.CreateLocalSupportFunctions(functions);
            }
            if (this._functions != null)
            {
                IntPtr nativeConfigurationSystem = this._functions.GetNativeConfigurationSystem();
                if (IntPtr.Zero != nativeConfigurationSystem)
                {
                    UnsafeIISMethods.MgdSetNativeConfiguration(nativeConfigurationSystem);
                }
            }
            this._mapPathCacheLock = new object();
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
                str = ((UnsafeIISMethods.MgdGetAppPathForPath(IntPtr.Zero, result, path.VirtualPathString, out zero, out cchPath) == 0) && (cchPath > 0)) ? StringUtil.StringFromWCharPtr(zero, cchPath) : null;
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
                        try
                        {
                            uint num;
                            string originalResult = null;
                            if (uint.TryParse(siteID, out num))
                            {
                                originalResult = ProcessHostConfigUtils.MapPathActual(ProcessHostConfigUtils.GetSiteNameFromId(num), path);
                            }
                            if (((originalResult != null) && (originalResult.Length == 2)) && (originalResult[1] == ':'))
                            {
                                originalResult = originalResult + @"\";
                            }
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
                            info.MapPathResult = originalResult;
                        }
                        catch (Exception exception)
                        {
                            if (!HttpRuntime.IsMapPathRelaxed)
                            {
                                info.CachedException = exception;
                                info.Evaluated = true;
                                throw;
                            }
                            info.MapPathResult = HttpRuntime.GetRelaxedMapPathResult(null);
                        }
                        info.Evaluated = true;
                    }
                }
            }
            if (info.CachedException != null)
            {
                throw info.CachedException;
            }
            return this.MatchResult(path2, info.MapPathResult);
        }

        private string MapPathWorker(string siteID, VirtualPath path)
        {
            return this.MapPathCaching(siteID, path);
        }

        private string MatchResult(VirtualPath path, string result)
        {
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Replace('/', '\\');
                if (path.HasTrailingSlash)
                {
                    if (!UrlPath.PathEndsWithExtraSlash(result))
                    {
                        result = result + @"\";
                    }
                    return result;
                }
                if (UrlPath.PathEndsWithExtraSlash(result))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }
            return result;
        }

        string IConfigMapPath.GetAppPathForPath(string siteID, string path)
        {
            return this.GetAppPathForPathWorker(siteID, VirtualPath.Create(path)).VirtualPathString;
        }

        void IConfigMapPath.GetDefaultSiteNameAndID(out string siteName, out string siteID)
        {
            siteID = "1";
            siteName = ProcessHostConfigUtils.GetSiteNameFromId(1);
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
            string rootWebConfigFilename = null;
            if (this._functions != null)
            {
                rootWebConfigFilename = this._functions.GetRootWebConfigFilename();
            }
            if (string.IsNullOrEmpty(rootWebConfigFilename))
            {
                rootWebConfigFilename = HttpConfigurationSystem.RootWebConfigurationFilePath;
            }
            return rootWebConfigFilename;
        }

        string IConfigMapPath.MapPath(string siteID, string path)
        {
            return this.MapPathWorker(siteID, VirtualPath.Create(path));
        }

        void IConfigMapPath.ResolveSiteArgument(string siteArgument, out string siteName, out string siteID)
        {
            if ((string.IsNullOrEmpty(siteArgument) || StringUtil.EqualsIgnoreCase(siteArgument, "1")) || StringUtil.EqualsIgnoreCase(siteArgument, ProcessHostConfigUtils.GetSiteNameFromId(1)))
            {
                siteName = ProcessHostConfigUtils.GetSiteNameFromId(1);
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
                        siteNameFromId = ProcessHostConfigUtils.GetSiteNameFromId(num);
                    }
                }
                else
                {
                    uint num2 = UnsafeIISMethods.MgdResolveSiteName(IntPtr.Zero, siteArgument);
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
    }
}

