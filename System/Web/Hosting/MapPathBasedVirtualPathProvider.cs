namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;

    internal class MapPathBasedVirtualPathProvider : VirtualPathProvider
    {
        private static string _AppRoot;

        private bool CacheLookupOrInsert(string virtualPath, bool isFile)
        {
            string physicalPath = HostingEnvironment.MapPathInternal(virtualPath);
            bool doNotCacheUrlMetadata = CachedPathData.DoNotCacheUrlMetadata;
            string key = null;
            if (!doNotCacheUrlMetadata)
            {
                key = this.CreateCacheKey(isFile, physicalPath);
                bool? nullable = HttpRuntime.CacheInternal[key] as bool?;
                if (nullable.HasValue)
                {
                    return nullable.Value;
                }
            }
            bool flag2 = isFile ? File.Exists(physicalPath) : Directory.Exists(physicalPath);
            if (!doNotCacheUrlMetadata)
            {
                CacheDependency dependencies = null;
                string filename = flag2 ? physicalPath : FileUtil.GetFirstExistingDirectory(AppRoot, physicalPath);
                if (filename != null)
                {
                    dependencies = new CacheDependency(filename);
                    TimeSpan urlMetadataSlidingExpiration = CachedPathData.UrlMetadataSlidingExpiration;
                    HttpRuntime.CacheInternal.UtcInsert(key, flag2, dependencies, Cache.NoAbsoluteExpiration, urlMetadataSlidingExpiration);
                }
            }
            return flag2;
        }

        private string CreateCacheKey(bool isFile, string physicalPath)
        {
            if (isFile)
            {
                return ("Bf" + physicalPath);
            }
            return ("Bd" + physicalPath);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            return this.CacheLookupOrInsert(virtualDir, false);
        }

        public override bool FileExists(string virtualPath)
        {
            return this.CacheLookupOrInsert(virtualPath, true);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (virtualPathDependencies == null)
            {
                return null;
            }
            StringCollection strings = null;
            foreach (string str in virtualPathDependencies)
            {
                string str2 = HostingEnvironment.MapPathInternal(str);
                if (strings == null)
                {
                    strings = new StringCollection();
                }
                strings.Add(str2);
            }
            if (strings == null)
            {
                return null;
            }
            string[] array = new string[strings.Count];
            strings.CopyTo(array, 0);
            return new CacheDependency(0, array, utcStart);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            return new MapPathBasedVirtualDirectory(virtualDir);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            return new MapPathBasedVirtualFile(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            HashCodeCombiner combiner = new HashCodeCombiner();
            foreach (string str in virtualPathDependencies)
            {
                string fileName = HostingEnvironment.MapPathInternal(str);
                combiner.AddFile(fileName);
            }
            return combiner.CombinedHashString;
        }

        private static string AppRoot
        {
            get
            {
                string str = _AppRoot;
                if (str == null)
                {
                    InternalSecurityPermissions.AppPathDiscovery.Assert();
                    str = FileUtil.RemoveTrailingDirectoryBackSlash(Path.GetFullPath(HttpRuntime.AppDomainAppPathInternal));
                    _AppRoot = str;
                }
                return str;
            }
        }
    }
}

