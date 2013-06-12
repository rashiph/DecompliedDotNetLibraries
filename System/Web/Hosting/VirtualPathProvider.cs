namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;

    public abstract class VirtualPathProvider : MarshalByRefObject
    {
        private VirtualPathProvider _previous;

        protected VirtualPathProvider()
        {
        }

        public virtual string CombineVirtualPaths(string basePath, string relativePath)
        {
            string basepath = null;
            if (!string.IsNullOrEmpty(basePath))
            {
                basepath = UrlPath.GetDirectory(basePath);
            }
            return UrlPath.Combine(basepath, relativePath);
        }

        internal VirtualPath CombineVirtualPaths(VirtualPath basePath, VirtualPath relativePath)
        {
            return VirtualPath.Create(this.CombineVirtualPaths(basePath.VirtualPathString, relativePath.VirtualPathString));
        }

        internal static VirtualPath CombineVirtualPathsInternal(VirtualPath basePath, VirtualPath relativePath)
        {
            VirtualPathProvider virtualPathProvider = HostingEnvironment.VirtualPathProvider;
            if (virtualPathProvider != null)
            {
                return virtualPathProvider.CombineVirtualPaths(basePath, relativePath);
            }
            return basePath.Parent.Combine(relativePath);
        }

        public virtual bool DirectoryExists(string virtualDir)
        {
            if (this._previous == null)
            {
                return false;
            }
            return this._previous.DirectoryExists(virtualDir);
        }

        internal bool DirectoryExists(VirtualPath virtualDir)
        {
            return this.DirectoryExists(virtualDir.VirtualPathString);
        }

        internal static bool DirectoryExistsNoThrow(string virtualDir)
        {
            try
            {
                return HostingEnvironment.VirtualPathProvider.DirectoryExists(virtualDir);
            }
            catch
            {
                return false;
            }
        }

        internal static bool DirectoryExistsNoThrow(VirtualPath virtualDir)
        {
            return DirectoryExistsNoThrow(virtualDir.VirtualPathString);
        }

        public virtual bool FileExists(string virtualPath)
        {
            if (this._previous == null)
            {
                return false;
            }
            return this._previous.FileExists(virtualPath);
        }

        internal bool FileExists(VirtualPath virtualPath)
        {
            return this.FileExists(virtualPath.VirtualPathString);
        }

        internal static CacheDependency GetCacheDependency(VirtualPath virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.GetCacheDependency(virtualPath, new SingleObjectCollection(virtualPath.VirtualPathString), DateTime.MaxValue);
        }

        public virtual CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (this._previous == null)
            {
                return null;
            }
            return this._previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        internal CacheDependency GetCacheDependency(VirtualPath virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return this.GetCacheDependency(virtualPath.VirtualPathString, virtualPathDependencies, utcStart);
        }

        public virtual string GetCacheKey(string virtualPath)
        {
            return null;
        }

        internal string GetCacheKey(VirtualPath virtualPath)
        {
            return this.GetCacheKey(virtualPath.VirtualPathString);
        }

        public virtual VirtualDirectory GetDirectory(string virtualDir)
        {
            if (this._previous == null)
            {
                return null;
            }
            return this._previous.GetDirectory(virtualDir);
        }

        internal VirtualDirectory GetDirectory(VirtualPath virtualDir)
        {
            return this.GetDirectoryWithCheck(virtualDir.VirtualPathString);
        }

        internal VirtualDirectory GetDirectoryWithCheck(string virtualPath)
        {
            VirtualDirectory directory = this.GetDirectory(virtualPath);
            if (directory == null)
            {
                return null;
            }
            if (!StringUtil.EqualsIgnoreCase(virtualPath, directory.VirtualPath))
            {
                throw new HttpException(System.Web.SR.GetString("Bad_VirtualPath_in_VirtualFileBase", new object[] { "VirtualDirectory", directory.VirtualPath, virtualPath }));
            }
            return directory;
        }

        public virtual VirtualFile GetFile(string virtualPath)
        {
            if (this._previous == null)
            {
                return null;
            }
            return this._previous.GetFile(virtualPath);
        }

        internal VirtualFile GetFile(VirtualPath virtualPath)
        {
            return this.GetFileWithCheck(virtualPath.VirtualPathString);
        }

        public virtual string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            if (this._previous == null)
            {
                return null;
            }
            return this._previous.GetFileHash(virtualPath, virtualPathDependencies);
        }

        internal string GetFileHash(VirtualPath virtualPath, IEnumerable virtualPathDependencies)
        {
            return this.GetFileHash(virtualPath.VirtualPathString, virtualPathDependencies);
        }

        internal VirtualFile GetFileWithCheck(string virtualPath)
        {
            VirtualFile file = this.GetFile(virtualPath);
            if (file == null)
            {
                return null;
            }
            if (!StringUtil.EqualsIgnoreCase(virtualPath, file.VirtualPath))
            {
                throw new HttpException(System.Web.SR.GetString("Bad_VirtualPath_in_VirtualFileBase", new object[] { "VirtualFile", file.VirtualPath, virtualPath }));
            }
            return file;
        }

        protected virtual void Initialize()
        {
        }

        internal virtual void Initialize(VirtualPathProvider previous)
        {
            this._previous = previous;
            this.Initialize();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public static Stream OpenFile(string virtualPath)
        {
            return HostingEnvironment.VirtualPathProvider.GetFileWithCheck(virtualPath).Open();
        }

        internal static Stream OpenFile(VirtualPath virtualPath)
        {
            return OpenFile(virtualPath.VirtualPathString);
        }

        protected internal VirtualPathProvider Previous
        {
            get
            {
                return this._previous;
            }
        }
    }
}

