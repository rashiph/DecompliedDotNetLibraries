namespace System.Web
{
    using System;
    using System.Configuration;
    using System.Configuration.Internal;
    using System.IO;
    using System.Threading;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;

    internal class CachedPathData
    {
        private string _configPath;
        private System.Web.Util.SafeBitVector32 _flags;
        private HandlerMappingMemo _handlerMemo;
        private string _physicalPath;
        private System.Web.Configuration.RuntimeConfig _runtimeConfig = System.Web.Configuration.RuntimeConfig.GetErrorRuntimeConfig();
        private VirtualPath _virtualPath;
        internal const int FAnonymousAccessAllowed = 0x200;
        internal const int FAnonymousAccessChecked = 0x100;
        internal const int FClosed = 0x20;
        internal const int FCloseNeeded = 0x40;
        internal const int FCompletedFirstRequest = 2;
        internal const int FExists = 4;
        internal const int FInited = 1;
        internal const int FOwnsConfigRecord = 0x10;
        private static int s_appConfigPathLength = 0;
        private static CacheItemRemovedCallback s_callback = new CacheItemRemovedCallback(CachedPathData.OnCacheItemRemoved);
        private static bool s_doNotCacheUrlMetadata = false;
        private static TimeSpan s_urlMetadataSlidingExpiration = HostingEnvironmentSection.DefaultUrlMetadataSlidingExpiration;

        internal CachedPathData(string configPath, VirtualPath virtualPath, string physicalPath, bool exists)
        {
            this._configPath = configPath;
            this._virtualPath = virtualPath;
            this._physicalPath = physicalPath;
            this._flags[4] = exists;
            string schemeDelimiter = Uri.SchemeDelimiter;
        }

        private void Close()
        {
            if ((this._flags[1] && this._flags.ChangeValue(0x20, true)) && this._flags[0x10])
            {
                this.ConfigRecord.Remove();
            }
        }

        private static string CreateKey(string configPath)
        {
            return ("d" + configPath);
        }

        internal static CachedPathData GetApplicationPathData()
        {
            if (!HostingEnvironment.IsHosted)
            {
                return GetRootWebPathData();
            }
            return GetConfigPathData(HostingEnvironment.AppConfigPath);
        }

        private static CachedPathData GetConfigPathData(string configPath)
        {
            bool exists = false;
            bool isDirectory = false;
            bool flag3 = IsCachedPathDataRemovable(configPath);
            if (flag3 && DoNotCacheUrlMetadata)
            {
                string str = null;
                VirtualPath path = null;
                string physicalPath = null;
                WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out str, out path);
                physicalPath = GetPhysicalPath(path);
                CachedPathData configPathData = GetConfigPathData(System.Configuration.ConfigPathUtility.GetParent(configPath));
                if (!string.IsNullOrEmpty(physicalPath))
                {
                    System.Web.Util.FileUtil.PhysicalPathStatus(physicalPath, false, false, out exists, out isDirectory);
                }
                CachedPathData data2 = new CachedPathData(configPath, path, physicalPath, exists);
                data2.Init(configPathData);
                return data2;
            }
            string key = CreateKey(configPath);
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            CachedPathData data3 = (CachedPathData) cacheInternal.Get(key);
            if (data3 != null)
            {
                data3.WaitForInit();
                return data3;
            }
            bool flag4 = false;
            string siteID = null;
            VirtualPath vpath = null;
            CachedPathData parentData = null;
            CacheDependency dependencies = null;
            string str6 = null;
            string[] filenames = null;
            string[] cachekeys = null;
            if (WebConfigurationHost.IsMachineConfigPath(configPath))
            {
                flag4 = true;
            }
            else
            {
                string parent = System.Configuration.ConfigPathUtility.GetParent(configPath);
                parentData = GetConfigPathData(parent);
                string str8 = CreateKey(parent);
                cachekeys = new string[] { str8 };
                if (!WebConfigurationHost.IsVirtualPathConfigPath(configPath))
                {
                    flag4 = true;
                }
                else
                {
                    flag4 = !flag3;
                    WebConfigurationHost.GetSiteIDAndVPathFromConfigPath(configPath, out siteID, out vpath);
                    str6 = GetPhysicalPath(vpath);
                    if (!string.IsNullOrEmpty(str6))
                    {
                        System.Web.Util.FileUtil.PhysicalPathStatus(str6, false, false, out exists, out isDirectory);
                        if (exists && !isDirectory)
                        {
                            filenames = new string[] { str6 };
                        }
                    }
                }
                try
                {
                    dependencies = new CacheDependency(0, filenames, cachekeys);
                }
                catch
                {
                }
            }
            CachedPathData data5 = null;
            bool flag5 = false;
            bool flag6 = false;
            CacheItemPriority priority = flag4 ? CacheItemPriority.NotRemovable : CacheItemPriority.Normal;
            TimeSpan slidingExpiration = flag4 ? Cache.NoSlidingExpiration : UrlMetadataSlidingExpiration;
            try
            {
                using (dependencies)
                {
                    data5 = new CachedPathData(configPath, vpath, str6, exists);
                    try
                    {
                    }
                    finally
                    {
                        data3 = (CachedPathData) cacheInternal.UtcAdd(key, data5, dependencies, Cache.NoAbsoluteExpiration, slidingExpiration, priority, s_callback);
                        if (data3 == null)
                        {
                            flag5 = true;
                        }
                    }
                }
                if (!flag5)
                {
                    data3.WaitForInit();
                    return data3;
                }
                lock (data5)
                {
                    try
                    {
                        data5.Init(parentData);
                        flag6 = true;
                    }
                    finally
                    {
                        data5._flags[1] = true;
                        Monitor.PulseAll(data5);
                        if (data5._flags[0x40])
                        {
                            data5.Close();
                        }
                    }
                    return data5;
                }
            }
            finally
            {
                if (flag5)
                {
                    if (!data5._flags[1])
                    {
                        lock (data5)
                        {
                            data5._flags[1] = true;
                            Monitor.PulseAll(data5);
                            if (data5._flags[0x40])
                            {
                                data5.Close();
                            }
                        }
                    }
                    if (!flag6 || ((data5.ConfigRecord != null) && data5.ConfigRecord.HasInitErrors))
                    {
                        if (dependencies != null)
                        {
                            if (!flag6)
                            {
                                dependencies = new CacheDependency(0, null, cachekeys);
                            }
                            else
                            {
                                dependencies = new CacheDependency(0, filenames, cachekeys);
                            }
                        }
                        using (dependencies)
                        {
                            cacheInternal.UtcInsert(key, data5, dependencies, DateTime.UtcNow.AddSeconds(5.0), Cache.NoSlidingExpiration, CacheItemPriority.Normal, s_callback);
                        }
                    }
                }
            }
            return data5;
        }

        internal static CachedPathData GetMachinePathData()
        {
            return GetConfigPathData("machine");
        }

        private static string GetPhysicalPath(VirtualPath virtualPath)
        {
            string physicalPath = null;
            try
            {
                physicalPath = virtualPath.MapPathInternal(true);
            }
            catch (HttpException exception)
            {
                if (exception.GetHttpCode() == 500)
                {
                    throw new HttpException(0x194, string.Empty);
                }
                throw;
            }
            System.Web.Util.FileUtil.CheckSuspiciousPhysicalPath(physicalPath);
            return physicalPath;
        }

        internal static CachedPathData GetRootWebPathData()
        {
            return GetConfigPathData("machine/webroot");
        }

        internal static CachedPathData GetVirtualPathData(VirtualPath virtualPath, bool permitPathsOutsideApp)
        {
            if (!HostingEnvironment.IsHosted)
            {
                return GetRootWebPathData();
            }
            if (virtualPath != null)
            {
                virtualPath.FailIfRelativePath();
            }
            if ((virtualPath != null) && virtualPath.IsWithinAppRoot)
            {
                return GetConfigPathData(WebConfigurationHost.GetConfigPathFromSiteIDAndVPath(HostingEnvironment.SiteID, virtualPath));
            }
            if (!permitPathsOutsideApp)
            {
                throw new ArgumentException(System.Web.SR.GetString("Cross_app_not_allowed", new object[] { (virtualPath != null) ? virtualPath.VirtualPathString : "null" }));
            }
            return GetApplicationPathData();
        }

        private void Init(CachedPathData parentData)
        {
            if (!HttpConfigurationSystem.UseHttpConfigurationSystem)
            {
                this._runtimeConfig = null;
            }
            else
            {
                IInternalConfigRecord uniqueConfigRecord = HttpConfigurationSystem.GetUniqueConfigRecord(this._configPath);
                if (uniqueConfigRecord.ConfigPath.Length == this._configPath.Length)
                {
                    this._flags[0x10] = true;
                    this._runtimeConfig = new System.Web.Configuration.RuntimeConfig(uniqueConfigRecord);
                }
                else
                {
                    this._runtimeConfig = parentData._runtimeConfig;
                }
            }
        }

        internal static void InitializeUrlMetadataSlidingExpiration(HostingEnvironmentSection section)
        {
            TimeSpan urlMetadataSlidingExpiration = section.UrlMetadataSlidingExpiration;
            if (urlMetadataSlidingExpiration == TimeSpan.Zero)
            {
                s_doNotCacheUrlMetadata = true;
            }
            else if (urlMetadataSlidingExpiration == TimeSpan.MaxValue)
            {
                s_urlMetadataSlidingExpiration = Cache.NoSlidingExpiration;
                s_doNotCacheUrlMetadata = false;
            }
            else
            {
                s_urlMetadataSlidingExpiration = urlMetadataSlidingExpiration;
                s_doNotCacheUrlMetadata = false;
            }
        }

        private static bool IsCachedPathDataRemovable(string configPath)
        {
            if (s_appConfigPathLength == 0)
            {
                s_appConfigPathLength = HostingEnvironment.IsHosted ? HostingEnvironment.AppConfigPath.Length : "machine/webroot".Length;
            }
            return (configPath.Length > s_appConfigPathLength);
        }

        internal static void MarkCompleted(CachedPathData pathData)
        {
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string configPath = pathData._configPath;
        Label_000D:
            pathData.CompletedFirstRequest = true;
            configPath = System.Configuration.ConfigPathUtility.GetParent(configPath);
            if (configPath != null)
            {
                string key = CreateKey(configPath);
                pathData = (CachedPathData) cacheInternal.Get(key);
                if ((pathData != null) && !pathData.CompletedFirstRequest)
                {
                    goto Label_000D;
                }
            }
        }

        private static void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            CachedPathData data = (CachedPathData) value;
            data._flags[0x40] = true;
            data.Close();
        }

        internal static void RemoveBadPathData(CachedPathData pathData)
        {
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            string configPath = pathData._configPath;
            string key = CreateKey(configPath);
            while (((pathData != null) && !pathData.CompletedFirstRequest) && !pathData.Exists)
            {
                cacheInternal.Remove(key);
                configPath = System.Configuration.ConfigPathUtility.GetParent(configPath);
                if (configPath == null)
                {
                    return;
                }
                key = CreateKey(configPath);
                pathData = (CachedPathData) cacheInternal.Get(key);
            }
        }

        internal void ValidatePath(string physicalPath)
        {
            if (!string.IsNullOrEmpty(this._physicalPath) || !string.IsNullOrEmpty(physicalPath))
            {
                if (!string.IsNullOrEmpty(this._physicalPath) && !string.IsNullOrEmpty(physicalPath))
                {
                    if (this._physicalPath.Length == physicalPath.Length)
                    {
                        if (string.Compare(this._physicalPath, 0, physicalPath, 0, physicalPath.Length, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return;
                        }
                    }
                    else if ((this._physicalPath.Length - physicalPath.Length) == 1)
                    {
                        if ((this._physicalPath[this._physicalPath.Length - 1] == System.IO.Path.DirectorySeparatorChar) && (string.Compare(this._physicalPath, 0, physicalPath, 0, physicalPath.Length, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            return;
                        }
                    }
                    else if ((((physicalPath.Length - this._physicalPath.Length) == 1) && (physicalPath[physicalPath.Length - 1] == System.IO.Path.DirectorySeparatorChar)) && (string.Compare(this._physicalPath, 0, physicalPath, 0, this._physicalPath.Length, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return;
                    }
                }
                System.Web.Util.FileUtil.CheckSuspiciousPhysicalPath(physicalPath);
            }
        }

        private void WaitForInit()
        {
            if (!this._flags[1])
            {
                lock (this)
                {
                    if (!this._flags[1])
                    {
                        Monitor.Wait(this);
                    }
                }
            }
        }

        internal bool AnonymousAccessAllowed
        {
            get
            {
                return this._flags[0x200];
            }
            set
            {
                this._flags[0x200] = value;
            }
        }

        internal bool AnonymousAccessChecked
        {
            get
            {
                return this._flags[0x100];
            }
            set
            {
                this._flags[0x100] = value;
            }
        }

        internal HandlerMappingMemo CachedHandler
        {
            get
            {
                return this._handlerMemo;
            }
            set
            {
                this._handlerMemo = value;
            }
        }

        internal bool CompletedFirstRequest
        {
            get
            {
                return this._flags[2];
            }
            set
            {
                this._flags[2] = value;
            }
        }

        internal IInternalConfigRecord ConfigRecord
        {
            get
            {
                if (this._runtimeConfig == null)
                {
                    return null;
                }
                return this._runtimeConfig.ConfigRecord;
            }
        }

        internal static bool DoNotCacheUrlMetadata
        {
            get
            {
                return s_doNotCacheUrlMetadata;
            }
        }

        internal bool Exists
        {
            get
            {
                return this._flags[4];
            }
        }

        internal VirtualPath Path
        {
            get
            {
                return this._virtualPath;
            }
        }

        internal string PhysicalPath
        {
            get
            {
                return this._physicalPath;
            }
        }

        internal System.Web.Configuration.RuntimeConfig RuntimeConfig
        {
            get
            {
                return this._runtimeConfig;
            }
        }

        internal static TimeSpan UrlMetadataSlidingExpiration
        {
            get
            {
                return s_urlMetadataSlidingExpiration;
            }
        }
    }
}

