namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;

    internal class MemoryBuildResultCache : BuildResultCache
    {
        private CacheInternal _cache;
        private Hashtable _dependentAssemblies = new Hashtable();
        private CacheItemRemovedCallback _onRemoveCallback;

        internal MemoryBuildResultCache(CacheInternal cache)
        {
            this._cache = cache;
            AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.OnAssemblyLoad);
        }

        internal override void CacheBuildResult(string cacheKey, BuildResult result, long hashCode, DateTime utcStart)
        {
            if (!BuildResultCompiledType.UsesDelayLoadType(result))
            {
                ICollection virtualPathDependencies = result.VirtualPathDependencies;
                CacheDependency dependencies = null;
                if (virtualPathDependencies != null)
                {
                    dependencies = result.VirtualPath.GetCacheDependency(virtualPathDependencies, utcStart);
                    if (dependencies != null)
                    {
                        result.UsesCacheDependency = true;
                    }
                }
                if (result.CacheToMemory)
                {
                    CacheItemPriority normal;
                    BuildResultCompiledAssemblyBase base2 = result as BuildResultCompiledAssemblyBase;
                    if (((base2 != null) && (base2.ResultAssembly != null)) && !base2.UsesExistingAssembly)
                    {
                        string assemblyCacheKey = BuildResultCache.GetAssemblyCacheKey(base2.ResultAssembly);
                        Assembly assembly = (Assembly) this._cache.Get(assemblyCacheKey);
                        if (assembly == null)
                        {
                            this._cache.UtcInsert(assemblyCacheKey, base2.ResultAssembly, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                        }
                        CacheDependency dependency2 = new CacheDependency(0, null, new string[] { assemblyCacheKey });
                        if (dependencies != null)
                        {
                            AggregateCacheDependency dependency3 = new AggregateCacheDependency();
                            dependency3.Add(new CacheDependency[] { dependencies, dependency2 });
                            dependencies = dependency3;
                        }
                        else
                        {
                            dependencies = dependency2;
                        }
                    }
                    string memoryCacheKey = GetMemoryCacheKey(cacheKey);
                    if (result.IsUnloadable)
                    {
                        normal = CacheItemPriority.Normal;
                    }
                    else
                    {
                        normal = CacheItemPriority.NotRemovable;
                    }
                    CacheItemRemovedCallback onRemoveCallback = null;
                    if (result.ShutdownAppDomainOnChange || (result is BuildResultCompiledAssemblyBase))
                    {
                        if (this._onRemoveCallback == null)
                        {
                            this._onRemoveCallback = new CacheItemRemovedCallback(this.OnCacheItemRemoved);
                        }
                        onRemoveCallback = this._onRemoveCallback;
                    }
                    this._cache.UtcInsert(memoryCacheKey, result, dependencies, result.MemoryCacheExpiration, result.MemoryCacheSlidingExpiration, normal, onRemoveCallback);
                }
            }
        }

        internal override BuildResult GetBuildResult(string cacheKey, VirtualPath virtualPath, long hashCode, bool ensureIsUpToDate)
        {
            string memoryCacheKey = GetMemoryCacheKey(cacheKey);
            BuildResult result = (BuildResult) this._cache.Get(memoryCacheKey);
            if (result == null)
            {
                return null;
            }
            if (!result.UsesCacheDependency && !result.IsUpToDate(virtualPath, ensureIsUpToDate))
            {
                this._cache.Remove(memoryCacheKey);
                return null;
            }
            return result;
        }

        private static string GetMemoryCacheKey(string cacheKey)
        {
            return ("c" + cacheKey);
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Assembly loadedAssembly = args.LoadedAssembly;
            if (!loadedAssembly.GlobalAssemblyCache)
            {
                string str = loadedAssembly.GetName().Name;
                if (StringUtil.StringStartsWith(str, "App_"))
                {
                    foreach (AssemblyName name in loadedAssembly.GetReferencedAssemblies())
                    {
                        if (StringUtil.StringStartsWith(name.Name, "App_"))
                        {
                            lock (this._dependentAssemblies)
                            {
                                ArrayList list = this._dependentAssemblies[name.Name] as ArrayList;
                                if (list == null)
                                {
                                    list = new ArrayList();
                                    this._dependentAssemblies[name.Name] = list;
                                }
                                list.Add(str);
                            }
                        }
                    }
                }
            }
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason == CacheItemRemovedReason.DependencyChanged)
            {
                if (HostingEnvironment.ShutdownInitiated)
                {
                    this.RemoveAssemblyAndCleanupDependenciesShuttingDown(value as BuildResultCompiledAssembly);
                }
                else
                {
                    this.RemoveAssemblyAndCleanupDependencies(value as BuildResultCompiledAssemblyBase);
                    if (((BuildResult) value).ShutdownAppDomainOnChange)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(MemoryBuildResultCache.ShutdownCallBack), "BuildResult change, cache key=" + key);
                    }
                }
            }
        }

        private static void RemoveAssembly(string path)
        {
            FileInfo f = new FileInfo(path);
            DiskBuildResultCache.RemoveAssembly(f);
            string str = Path.ChangeExtension(f.FullName, ".pdb");
            if (File.Exists(str))
            {
                DiskBuildResultCache.TryDeleteFile(new FileInfo(str));
            }
        }

        private void RemoveAssemblyAndCleanupDependencies(string assemblyName)
        {
            bool gotLock = false;
            try
            {
                CompilationLock.GetLock(ref gotLock);
                lock (this._dependentAssemblies)
                {
                    this.RemoveAssemblyAndCleanupDependenciesNoLock(assemblyName);
                }
            }
            finally
            {
                if (gotLock)
                {
                    CompilationLock.ReleaseLock();
                }
                DiskBuildResultCache.ShutDownAppDomainIfRequired();
            }
        }

        internal void RemoveAssemblyAndCleanupDependencies(BuildResultCompiledAssemblyBase compiledResult)
        {
            if ((compiledResult != null) && (((compiledResult != null) && (compiledResult.ResultAssembly != null)) && !compiledResult.UsesExistingAssembly))
            {
                this.RemoveAssemblyAndCleanupDependencies(compiledResult.ResultAssembly.GetName().Name);
            }
        }

        private void RemoveAssemblyAndCleanupDependenciesNoLock(string assemblyName)
        {
            string assemblyCacheKeyFromName = BuildResultCache.GetAssemblyCacheKeyFromName(assemblyName);
            Assembly assembly = (Assembly) this._cache[assemblyCacheKeyFromName];
            if (assembly != null)
            {
                string assemblyCodeBase = Util.GetAssemblyCodeBase(assembly);
                this._cache.Remove(assemblyCacheKeyFromName);
                ICollection is2 = this._dependentAssemblies[assemblyName] as ICollection;
                if (is2 != null)
                {
                    foreach (string str3 in is2)
                    {
                        this.RemoveAssemblyAndCleanupDependenciesNoLock(str3);
                    }
                    this._dependentAssemblies.Remove(assemblyCacheKeyFromName);
                }
                RemoveAssembly(assemblyCodeBase);
            }
        }

        internal void RemoveAssemblyAndCleanupDependenciesShuttingDown(BuildResultCompiledAssemblyBase compiledResult)
        {
            if ((compiledResult != null) && (((compiledResult != null) && (compiledResult.ResultAssembly != null)) && !compiledResult.UsesExistingAssembly))
            {
                string name = compiledResult.ResultAssembly.GetName().Name;
                lock (this._dependentAssemblies)
                {
                    this.RemoveAssemblyAndCleanupDependenciesNoLock(name);
                }
            }
        }

        private static void ShutdownCallBack(object state)
        {
            string message = state as string;
            if (message != null)
            {
                HttpRuntime.SetShutdownReason(ApplicationShutdownReason.BuildManagerChange, message);
            }
            HostingEnvironment.InitiateShutdownWithoutDemand();
        }
    }
}

