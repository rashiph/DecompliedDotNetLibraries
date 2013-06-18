namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Management;
    using System.Web.UI;

    public static class OutputCache
    {
        internal const string ASPNET_INTERNAL_PROVIDER_NAME = "AspNetInternalProvider";
        private const string OUTPUTCACHE_KEYPREFIX_DEPENDENCIES = "aD";
        private static int s_cEntries;
        private static OutputCacheProvider s_defaultProvider;
        private static CacheItemRemovedCallback s_dependencyRemovedCallback;
        private static CacheItemRemovedCallback s_dependencyRemovedCallbackForFragment;
        private static CacheItemRemovedCallback s_entryRemovedCallback;
        private static bool s_inited;
        private static object s_initLock = new object();
        private static OutputCacheProviderCollection s_providers;

        private static void AddCacheKeyToDependencies(ref CacheDependency dependencies, string cacheKey)
        {
            CacheDependency dependency = new CacheDependency(0, null, new string[] { cacheKey });
            if (dependencies == null)
            {
                dependencies = dependency;
            }
            else
            {
                AggregateCacheDependency dependency2 = dependencies as AggregateCacheDependency;
                if (dependency2 != null)
                {
                    dependency2.Add(new CacheDependency[] { dependency });
                }
                else
                {
                    dependency2 = new AggregateCacheDependency();
                    dependency2.Add(new CacheDependency[] { dependency, dependencies });
                    dependencies = dependency2;
                }
            }
        }

        private static CachedRawResponse Convert(OutputCacheEntry oce)
        {
            ArrayList headers = null;
            if ((oce.HeaderElements != null) && (oce.HeaderElements.Count > 0))
            {
                headers = new ArrayList(oce.HeaderElements.Count);
                for (int i = 0; i < oce.HeaderElements.Count; i++)
                {
                    HttpResponseHeader header = new HttpResponseHeader(oce.HeaderElements[i].Name, oce.HeaderElements[i].Value);
                    headers.Add(header);
                }
            }
            ArrayList buffers = null;
            if ((oce.ResponseElements != null) && (oce.ResponseElements.Count > 0))
            {
                buffers = new ArrayList(oce.ResponseElements.Count);
                for (int j = 0; j < oce.ResponseElements.Count; j++)
                {
                    ResponseElement element = oce.ResponseElements[j];
                    IHttpResponseElement element2 = null;
                    if (element is FileResponseElement)
                    {
                        HttpContext current = HttpContext.Current;
                        HttpWorkerRequest request = (current != null) ? current.WorkerRequest : null;
                        bool supportsLongTransmitFile = (request != null) && request.SupportsLongTransmitFile;
                        bool isImpersonating = ((current != null) && current.IsClientImpersonationConfigured) || HttpRuntime.IsOnUNCShareInternal;
                        FileResponseElement element3 = (FileResponseElement) element;
                        element2 = new HttpFileResponseElement(element3.Path, element3.Offset, element3.Length, isImpersonating, supportsLongTransmitFile);
                    }
                    else if (element is MemoryResponseElement)
                    {
                        MemoryResponseElement element4 = (MemoryResponseElement) element;
                        int size = System.Convert.ToInt32(element4.Length);
                        element2 = new HttpResponseBufferElement(element4.Buffer, size);
                    }
                    else
                    {
                        if (!(element is SubstitutionResponseElement))
                        {
                            throw new NotSupportedException();
                        }
                        SubstitutionResponseElement element5 = (SubstitutionResponseElement) element;
                        element2 = new HttpSubstBlockResponseElement(element5.Callback);
                    }
                    buffers.Add(element2);
                }
            }
            else
            {
                buffers = new ArrayList();
            }
            return new CachedRawResponse(new HttpRawResponse(oce.StatusCode, oce.StatusDescription, headers, buffers, false), oce.Settings, oce.KernelCacheUrl, oce.CachedVaryId);
        }

        private static OutputCacheEntry Convert(CachedRawResponse cachedRawResponse, string depKey, string[] fileDependencies)
        {
            List<HeaderElement> headerElements = null;
            ArrayList headers = cachedRawResponse._rawResponse.Headers;
            int capacity = (headers != null) ? headers.Count : 0;
            for (int i = 0; i < capacity; i++)
            {
                if (headerElements == null)
                {
                    headerElements = new List<HeaderElement>(capacity);
                }
                HttpResponseHeader header = (HttpResponseHeader) headers[i];
                headerElements.Add(new HeaderElement(header.Name, header.Value));
            }
            List<ResponseElement> responseElements = null;
            ArrayList buffers = cachedRawResponse._rawResponse.Buffers;
            capacity = (buffers != null) ? buffers.Count : 0;
            for (int j = 0; j < capacity; j++)
            {
                if (responseElements == null)
                {
                    responseElements = new List<ResponseElement>(capacity);
                }
                IHttpResponseElement element = buffers[j] as IHttpResponseElement;
                if (element is HttpFileResponseElement)
                {
                    HttpFileResponseElement element2 = element as HttpFileResponseElement;
                    responseElements.Add(new FileResponseElement(element2.FileName, element2.Offset, element.GetSize()));
                }
                else if (element is HttpSubstBlockResponseElement)
                {
                    HttpSubstBlockResponseElement element3 = element as HttpSubstBlockResponseElement;
                    responseElements.Add(new SubstitutionResponseElement(element3.Callback));
                }
                else
                {
                    byte[] bytes = element.GetBytes();
                    long length = (bytes != null) ? ((long) bytes.Length) : ((long) 0);
                    responseElements.Add(new MemoryResponseElement(bytes, length));
                }
            }
            return new OutputCacheEntry(cachedRawResponse._cachedVaryId, cachedRawResponse._settings, cachedRawResponse._kernelCacheUrl, depKey, fileDependencies, cachedRawResponse._rawResponse.StatusCode, cachedRawResponse._rawResponse.StatusDescription, headerElements, responseElements);
        }

        private static void DecrementCount()
        {
            if (Providers == null)
            {
                Interlocked.Decrement(ref s_cEntries);
            }
        }

        private static void DependencyRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            DependencyCacheEntry entry = value as DependencyCacheEntry;
            if (entry.KernelCacheEntryKey != null)
            {
                if (HttpRuntime.UseIntegratedPipeline)
                {
                    UnsafeIISMethods.MgdFlushKernelCache(entry.KernelCacheEntryKey);
                }
                else
                {
                    System.Web.UnsafeNativeMethods.InvalidateKernelCache(entry.KernelCacheEntryKey);
                }
            }
            if ((reason == CacheItemRemovedReason.DependencyChanged) && (entry.OutputCacheEntryKey != null))
            {
                try
                {
                    RemoveFromProvider(entry.OutputCacheEntryKey, entry.ProviderName);
                }
                catch (Exception exception)
                {
                    HandleErrorWithoutContext(exception);
                }
            }
        }

        private static void DependencyRemovedCallbackForFragment(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason == CacheItemRemovedReason.DependencyChanged)
            {
                DependencyCacheEntry entry = value as DependencyCacheEntry;
                if (entry.OutputCacheEntryKey != null)
                {
                    try
                    {
                        RemoveFragment(entry.OutputCacheEntryKey, entry.ProviderName);
                    }
                    catch (Exception exception)
                    {
                        HandleErrorWithoutContext(exception);
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public static object Deserialize(Stream stream)
        {
            object obj2 = new BinaryFormatter().Deserialize(stream);
            if (((!(obj2 is OutputCacheEntry) && !(obj2 is PartialCachingCacheEntry)) && (!(obj2 is CachedVary) && !(obj2 is ControlCachedVary))) && ((!(obj2 is FileResponseElement) && !(obj2 is MemoryResponseElement)) && !(obj2 is SubstitutionResponseElement)))
            {
                throw new ArgumentException(System.Web.SR.GetString("OutputCacheExtensibility_CantSerializeDeserializeType"));
            }
            return obj2;
        }

        private static void EnsureInitialized()
        {
            if (!s_inited)
            {
                lock (s_initLock)
                {
                    if (!s_inited)
                    {
                        OutputCacheSection outputCache = RuntimeConfig.GetAppConfig().OutputCache;
                        s_providers = outputCache.CreateProviderCollection();
                        s_defaultProvider = outputCache.GetDefaultProvider(s_providers);
                        s_entryRemovedCallback = new CacheItemRemovedCallback(OutputCache.EntryRemovedCallback);
                        s_dependencyRemovedCallback = new CacheItemRemovedCallback(OutputCache.DependencyRemovedCallback);
                        s_dependencyRemovedCallbackForFragment = new CacheItemRemovedCallback(OutputCache.DependencyRemovedCallbackForFragment);
                        s_inited = true;
                    }
                }
            }
        }

        private static void EntryRemovedCallback(string key, object value, CacheItemRemovedReason reason)
        {
            DecrementCount();
            PerfCounters.DecrementCounter(AppPerfCounter.OUTPUT_CACHE_ENTRIES);
            PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_TURNOVER_RATE);
            CachedRawResponse response = value as CachedRawResponse;
            if (response != null)
            {
                string cacheKey = response._kernelCacheUrl;
                if ((cacheKey != null) && (HttpRuntime.CacheInternal.Get(key) == null))
                {
                    if (HttpRuntime.UseIntegratedPipeline)
                    {
                        UnsafeIISMethods.MgdFlushKernelCache(cacheKey);
                    }
                    else
                    {
                        System.Web.UnsafeNativeMethods.InvalidateKernelCache(cacheKey);
                    }
                }
            }
        }

        internal static object Get(string key)
        {
            object obj2 = null;
            OutputCacheProvider provider = GetProvider(HttpContext.Current);
            if (provider != null)
            {
                obj2 = provider.Get(key);
                OutputCacheEntry oce = obj2 as OutputCacheEntry;
                if (oce != null)
                {
                    if (HasDependencyChanged(false, oce.DependenciesKey, oce.Dependencies, oce.KernelCacheUrl, key, provider.Name))
                    {
                        RemoveFromProvider(key, provider.Name);
                        return null;
                    }
                    obj2 = Convert(oce);
                }
            }
            if (obj2 == null)
            {
                obj2 = HttpRuntime.CacheInternal.Get(key);
            }
            return obj2;
        }

        internal static object GetFragment(string key, string providerName)
        {
            object obj2 = null;
            OutputCacheProvider fragmentProvider = GetFragmentProvider(providerName);
            if (fragmentProvider != null)
            {
                obj2 = fragmentProvider.Get(key);
                PartialCachingCacheEntry entry = obj2 as PartialCachingCacheEntry;
                if ((entry != null) && HasDependencyChanged(true, entry._dependenciesKey, entry._dependencies, null, key, fragmentProvider.Name))
                {
                    RemoveFragment(key, fragmentProvider.Name);
                    return null;
                }
            }
            if (obj2 == null)
            {
                obj2 = HttpRuntime.CacheInternal.Get(key);
            }
            return obj2;
        }

        private static OutputCacheProvider GetFragmentProvider(string providerName)
        {
            OutputCacheProvider provider = null;
            if (providerName == null)
            {
                return s_defaultProvider;
            }
            provider = s_providers[providerName];
            if (provider == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_Not_Found", new object[] { providerName }));
            }
            return provider;
        }

        private static OutputCacheProvider GetProvider(HttpContext context)
        {
            if (context == null)
            {
                return null;
            }
            string outputCacheProviderName = context.ApplicationInstance.GetOutputCacheProviderName(context);
            switch (outputCacheProviderName)
            {
                case null:
                    throw new ProviderException(System.Web.SR.GetString("GetOutputCacheProviderName_Invalid", new object[] { outputCacheProviderName }));

                case "AspNetInternalProvider":
                    return null;
            }
            OutputCacheProvider provider = (s_providers == null) ? null : s_providers[outputCacheProviderName];
            if (provider == null)
            {
                throw new ProviderException(System.Web.SR.GetString("GetOutputCacheProviderName_Invalid", new object[] { outputCacheProviderName }));
            }
            return provider;
        }

        private static void HandleErrorWithoutContext(Exception e)
        {
            HttpApplicationFactory.RaiseError(e);
            try
            {
                WebBaseEvent.RaiseRuntimeError(e, typeof(OutputCache));
            }
            catch
            {
            }
        }

        internal static bool HasDependencyChanged(bool isFragment, string depKey, string[] fileDeps, string kernelKey, string oceKey, string providerName)
        {
            if (depKey == null)
            {
                return false;
            }
            if (HttpRuntime.CacheInternal.Get(depKey) != null)
            {
                return false;
            }
            CacheDependency dependencies = new CacheDependency(0, fileDeps);
            int length = "aD".Length;
            int num2 = depKey.Length - length;
            CacheItemRemovedCallback onRemoveCallback = isFragment ? s_dependencyRemovedCallbackForFragment : s_dependencyRemovedCallback;
            if (string.Compare(dependencies.GetUniqueID(), 0, depKey, length, num2, StringComparison.Ordinal) == 0)
            {
                HttpRuntime.CacheInternal.UtcInsert(depKey, new DependencyCacheEntry(oceKey, kernelKey, providerName), dependencies, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, onRemoveCallback);
                return false;
            }
            dependencies.Dispose();
            return true;
        }

        private static void IncrementCount()
        {
            if (Providers == null)
            {
                Interlocked.Increment(ref s_cEntries);
            }
        }

        internal static void InsertFragment(string cachedVaryKey, ControlCachedVary cachedVary, string fragmentKey, PartialCachingCacheEntry fragment, CacheDependency dependencies, DateTime absExp, TimeSpan slidingExp, string providerName)
        {
            OutputCacheProvider fragmentProvider = GetFragmentProvider(providerName);
            bool flag = fragmentProvider != null;
            if (flag)
            {
                bool flag2 = (slidingExp == Cache.NoSlidingExpiration) && ((dependencies == null) || dependencies.IsFileDependency());
                if (flag && !flag2)
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_does_not_support_policy_for_fragments", new object[] { providerName }));
                }
            }
            if (cachedVary != null)
            {
                ControlCachedVary vary;
                if (!flag)
                {
                    vary = UtcAdd(cachedVaryKey, cachedVary);
                }
                else
                {
                    vary = (ControlCachedVary) fragmentProvider.Add(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                }
                if (vary != null)
                {
                    if (!cachedVary.Equals(vary))
                    {
                        if (!flag)
                        {
                            HttpRuntime.CacheInternal.UtcInsert(cachedVaryKey, cachedVary);
                        }
                        else
                        {
                            fragmentProvider.Set(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                        }
                    }
                    else
                    {
                        cachedVary = vary;
                    }
                }
                if (!flag)
                {
                    AddCacheKeyToDependencies(ref dependencies, cachedVaryKey);
                }
                fragment._cachedVaryId = cachedVary.CachedVaryId;
            }
            if (!flag)
            {
                HttpRuntime.CacheInternal.UtcInsert(fragmentKey, fragment, dependencies, absExp, slidingExp, CacheItemPriority.Normal, null);
            }
            else
            {
                string key = null;
                if (dependencies != null)
                {
                    key = "aD" + dependencies.GetUniqueID();
                    fragment._dependenciesKey = key;
                    fragment._dependencies = dependencies.GetFileDependencies();
                }
                fragmentProvider.Set(fragmentKey, fragment, absExp);
                if ((dependencies != null) && (HttpRuntime.CacheInternal.UtcAdd(key, new DependencyCacheEntry(fragmentKey, null, fragmentProvider.Name), dependencies, absExp, Cache.NoSlidingExpiration, CacheItemPriority.Normal, s_dependencyRemovedCallbackForFragment) != null))
                {
                    dependencies.Dispose();
                }
            }
        }

        internal static void InsertResponse(string cachedVaryKey, CachedVary cachedVary, string rawResponseKey, CachedRawResponse rawResponse, CacheDependency dependencies, DateTime absExp, TimeSpan slidingExp)
        {
            OutputCacheProvider provider = GetProvider(HttpContext.Current);
            bool flag = provider != null;
            if (flag)
            {
                bool flag2 = ((IsSubstBlockSerializable(rawResponse._rawResponse) && rawResponse._settings.IsValidationCallbackSerializable()) && (slidingExp == Cache.NoSlidingExpiration)) && ((dependencies == null) || dependencies.IsFileDependency());
                if (flag && !flag2)
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_does_not_support_policy_for_responses", new object[] { provider.Name }));
                }
            }
            if (cachedVary != null)
            {
                CachedVary vary;
                if (!flag)
                {
                    vary = UtcAdd(cachedVaryKey, cachedVary);
                }
                else
                {
                    vary = (CachedVary) provider.Add(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                }
                if (vary != null)
                {
                    if (!cachedVary.Equals(vary))
                    {
                        if (!flag)
                        {
                            HttpRuntime.CacheInternal.UtcInsert(cachedVaryKey, cachedVary);
                        }
                        else
                        {
                            provider.Set(cachedVaryKey, cachedVary, Cache.NoAbsoluteExpiration);
                        }
                    }
                    else
                    {
                        cachedVary = vary;
                    }
                }
                if (!flag)
                {
                    AddCacheKeyToDependencies(ref dependencies, cachedVaryKey);
                }
                rawResponse._cachedVaryId = cachedVary.CachedVaryId;
            }
            if (!flag)
            {
                HttpRuntime.CacheInternal.UtcInsert(rawResponseKey, rawResponse, dependencies, absExp, slidingExp, CacheItemPriority.Normal, s_entryRemovedCallback);
                IncrementCount();
                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_ENTRIES);
                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_TURNOVER_RATE);
            }
            else
            {
                string depKey = null;
                string[] fileDependencies = null;
                if (dependencies != null)
                {
                    depKey = "aD" + dependencies.GetUniqueID();
                    fileDependencies = dependencies.GetFileDependencies();
                }
                OutputCacheEntry entry = Convert(rawResponse, depKey, fileDependencies);
                provider.Set(rawResponseKey, entry, absExp);
                if ((dependencies != null) && (HttpRuntime.CacheInternal.UtcAdd(depKey, new DependencyCacheEntry(rawResponseKey, entry.KernelCacheUrl, provider.Name), dependencies, absExp, Cache.NoSlidingExpiration, CacheItemPriority.Normal, s_dependencyRemovedCallback) != null))
                {
                    dependencies.Dispose();
                }
            }
        }

        private static bool IsSubstBlockSerializable(HttpRawResponse rawResponse)
        {
            if (rawResponse.HasSubstBlocks)
            {
                for (int i = 0; i < rawResponse.Buffers.Count; i++)
                {
                    HttpSubstBlockResponseElement element = rawResponse.Buffers[i] as HttpSubstBlockResponseElement;
                    if ((element != null) && !element.Callback.Method.IsStatic)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal static void Remove(string key, HttpContext context)
        {
            HttpRuntime.CacheInternal.Remove(key);
            if (context == null)
            {
                OutputCacheProviderCollection providers = Providers;
                if (providers != null)
                {
                    foreach (OutputCacheProvider provider in providers)
                    {
                        provider.Remove(key);
                    }
                }
            }
            else
            {
                OutputCacheProvider provider2 = GetProvider(context);
                if (provider2 != null)
                {
                    provider2.Remove(key);
                }
            }
        }

        internal static void RemoveFragment(string key, string providerName)
        {
            OutputCacheProvider fragmentProvider = GetFragmentProvider(providerName);
            if (fragmentProvider != null)
            {
                fragmentProvider.Remove(key);
            }
            HttpRuntime.CacheInternal.Remove(key);
        }

        internal static void RemoveFromProvider(string key, string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException("providerName");
            }
            OutputCacheProviderCollection providers = Providers;
            OutputCacheProvider provider = (providers == null) ? null : providers[providerName];
            if (provider == null)
            {
                throw new ProviderException(System.Web.SR.GetString("Provider_Not_Found", new object[] { providerName }));
            }
            provider.Remove(key);
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public static void Serialize(Stream stream, object data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            if (((!(data is OutputCacheEntry) && !(data is PartialCachingCacheEntry)) && (!(data is CachedVary) && !(data is ControlCachedVary))) && ((!(data is FileResponseElement) && !(data is MemoryResponseElement)) && !(data is SubstitutionResponseElement)))
            {
                throw new ArgumentException(System.Web.SR.GetString("OutputCacheExtensibility_CantSerializeDeserializeType"));
            }
            formatter.Serialize(stream, data);
        }

        internal static void ThrowIfProviderNotFound(string providerName)
        {
            if (providerName != null)
            {
                OutputCacheProviderCollection providers = Providers;
                if ((providers == null) || (providers[providerName] == null))
                {
                    throw new ProviderException(System.Web.SR.GetString("Provider_Not_Found", new object[] { providerName }));
                }
            }
        }

        private static CachedVary UtcAdd(string key, CachedVary cachedVary)
        {
            return (CachedVary) HttpRuntime.CacheInternal.UtcAdd(key, cachedVary, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        private static ControlCachedVary UtcAdd(string key, ControlCachedVary cachedVary)
        {
            return (ControlCachedVary) HttpRuntime.CacheInternal.UtcAdd(key, cachedVary, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public static string DefaultProviderName
        {
            get
            {
                EnsureInitialized();
                if (s_defaultProvider == null)
                {
                    return "AspNetInternalProvider";
                }
                return s_defaultProvider.Name;
            }
        }

        internal static bool InUse
        {
            get
            {
                return ((Providers != null) || (s_cEntries != 0));
            }
        }

        public static OutputCacheProviderCollection Providers
        {
            get
            {
                EnsureInitialized();
                return s_providers;
            }
        }
    }
}

