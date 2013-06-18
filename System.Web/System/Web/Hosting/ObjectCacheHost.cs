namespace System.Web.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Caching;
    using System.Runtime.Caching.Hosting;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Util;

    internal sealed class ObjectCacheHost : IServiceProvider, IApplicationIdentifier, IFileChangeNotificationSystem, IMemoryCacheManager
    {
        private Dictionary<MemoryCache, MemoryCacheInfo> _cacheInfos;
        private object _lock = new object();

        object IServiceProvider.GetService(Type service)
        {
            if (service == typeof(IFileChangeNotificationSystem))
            {
                return this;
            }
            if (service == typeof(IMemoryCacheManager))
            {
                return this;
            }
            if (service == typeof(IApplicationIdentifier))
            {
                return this;
            }
            return null;
        }

        string IApplicationIdentifier.GetApplicationId()
        {
            return HttpRuntime.AppDomainAppIdInternal;
        }

        void IFileChangeNotificationSystem.StartMonitoring(string filePath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWrite, out long fileSize)
        {
            FileAttributesData nonExistantAttributesData;
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (onChangedCallback == null)
            {
                throw new ArgumentNullException("onChangedCallback");
            }
            FileChangeEventTarget target = new FileChangeEventTarget(onChangedCallback);
            HttpRuntime.FileChangesMonitor.StartMonitoringPath(filePath, target.Handler, out nonExistantAttributesData);
            if (nonExistantAttributesData == null)
            {
                nonExistantAttributesData = FileAttributesData.NonExistantAttributesData;
            }
            state = target;
            lastWrite = nonExistantAttributesData.UtcLastWriteTime;
            fileSize = nonExistantAttributesData.FileSize;
        }

        void IFileChangeNotificationSystem.StopMonitoring(string filePath, object state)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            HttpRuntime.FileChangesMonitor.StopMonitoringPath(filePath, state);
        }

        void IMemoryCacheManager.ReleaseCache(MemoryCache memoryCache)
        {
            if (memoryCache == null)
            {
                throw new ArgumentNullException("memoryCache");
            }
            long sizeUpdate = 0L;
            lock (this._lock)
            {
                if (this._cacheInfos != null)
                {
                    MemoryCacheInfo info = null;
                    if (this._cacheInfos.TryGetValue(memoryCache, out info))
                    {
                        sizeUpdate = -info.Size;
                        this._cacheInfos.Remove(memoryCache);
                    }
                }
            }
            if (sizeUpdate != 0L)
            {
                ApplicationManager applicationManager = HostingEnvironment.GetApplicationManager();
                if (applicationManager != null)
                {
                    applicationManager.GetUpdatedTotalCacheSize(sizeUpdate);
                }
            }
        }

        void IMemoryCacheManager.UpdateCacheSize(long size, MemoryCache memoryCache)
        {
            if (memoryCache == null)
            {
                throw new ArgumentNullException("memoryCache");
            }
            long sizeUpdate = 0L;
            lock (this._lock)
            {
                if (this._cacheInfos == null)
                {
                    this._cacheInfos = new Dictionary<MemoryCache, MemoryCacheInfo>();
                }
                MemoryCacheInfo info = null;
                if (!this._cacheInfos.TryGetValue(memoryCache, out info))
                {
                    info = new MemoryCacheInfo {
                        Cache = memoryCache
                    };
                    this._cacheInfos[memoryCache] = info;
                }
                sizeUpdate = size - info.Size;
                info.Size = size;
            }
            ApplicationManager applicationManager = HostingEnvironment.GetApplicationManager();
            if (applicationManager != null)
            {
                applicationManager.GetUpdatedTotalCacheSize(sizeUpdate);
            }
        }

        internal long TrimCache(int percent)
        {
            long num = 0L;
            Dictionary<MemoryCache, MemoryCacheInfo>.KeyCollection keys = null;
            lock (this._lock)
            {
                if ((this._cacheInfos != null) && (this._cacheInfos.Count > 0))
                {
                    keys = this._cacheInfos.Keys;
                }
            }
            if (keys != null)
            {
                foreach (MemoryCache cache in keys)
                {
                    num += cache.Trim(percent);
                }
            }
            return num;
        }

        internal sealed class FileChangeEventTarget
        {
            private FileChangeEventHandler _handler;
            private OnChangedCallback _onChangedCallback;

            internal FileChangeEventTarget(OnChangedCallback onChangedCallback)
            {
                this._onChangedCallback = onChangedCallback;
                this._handler = new FileChangeEventHandler(this.OnChanged);
            }

            private void OnChanged(object sender, FileChangeEvent e)
            {
                this._onChangedCallback(null);
            }

            internal FileChangeEventHandler Handler
            {
                get
                {
                    return this._handler;
                }
            }
        }

        internal sealed class MemoryCacheInfo
        {
            internal MemoryCache Cache;
            internal long Size;
        }
    }
}

