namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.Util;

    [Serializable]
    internal sealed class VirtualPath : IComparable
    {
        private string _appRelativeVirtualPath;
        private string _virtualPath;
        private const int appRelativeAttempted = 4;
        private SimpleBitVector32 flags;
        private const int isWithinAppRoot = 2;
        private const int isWithinAppRootComputed = 1;
        internal static VirtualPath RootVirtualPath = Create("/");

        private VirtualPath()
        {
        }

        private VirtualPath(string virtualPath)
        {
            if (UrlPath.IsAppRelativePath(virtualPath))
            {
                this._appRelativeVirtualPath = virtualPath;
            }
            else
            {
                this._virtualPath = virtualPath;
            }
        }

        public VirtualPath Combine(VirtualPath relativePath)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException("relativePath");
            }
            if (!relativePath.IsRelative)
            {
                return relativePath;
            }
            this.FailIfRelativePath();
            return new VirtualPath(UrlPath.Combine(this.VirtualPathStringWhicheverAvailable, relativePath.VirtualPathString));
        }

        internal static VirtualPath Combine(VirtualPath v1, VirtualPath v2)
        {
            if (v1 == null)
            {
                v1 = HttpRuntime.AppDomainAppVirtualPathObject;
            }
            if (v1 == null)
            {
                v2.FailIfRelativePath();
                return v2;
            }
            return v1.Combine(v2);
        }

        public VirtualPath CombineWithAppRoot()
        {
            return HttpRuntime.AppDomainAppVirtualPathObject.Combine(this);
        }

        private void CopyFlagsFrom(VirtualPath virtualPath, int mask)
        {
            this.flags.IntegerValue |= virtualPath.flags.IntegerValue & mask;
        }

        public static VirtualPath Create(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAllPath);
        }

        public static unsafe VirtualPath Create(string virtualPath, VirtualPathOptions options)
        {
            if (virtualPath != null)
            {
                virtualPath = virtualPath.Trim();
            }
            if (string.IsNullOrEmpty(virtualPath))
            {
                if ((options & VirtualPathOptions.AllowNull) == 0)
                {
                    throw new ArgumentNullException("virtualPath");
                }
                return null;
            }
            bool flag = false;
            bool flag2 = false;
            int length = virtualPath.Length;
            fixed (char* str = ((char*) virtualPath))
            {
                char* chPtr = str;
                for (int i = 0; i < length; i++)
                {
                    switch (chPtr[i])
                    {
                        case '.':
                            flag2 = true;
                            break;

                        case '/':
                            if ((i > 0) && (chPtr[i - 1] == '/'))
                            {
                                flag = true;
                            }
                            break;

                        case '\\':
                            flag = true;
                            break;

                        case '\0':
                            throw new HttpException(System.Web.SR.GetString("Invalid_vpath", new object[] { virtualPath }));
                    }
                }
            }
            if (flag)
            {
                if ((options & VirtualPathOptions.FailIfMalformed) != 0)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_vpath", new object[] { virtualPath }));
                }
                virtualPath = UrlPath.FixVirtualPathSlashes(virtualPath);
            }
            if ((options & VirtualPathOptions.EnsureTrailingSlash) != 0)
            {
                virtualPath = UrlPath.AppendSlashToPathIfNeeded(virtualPath);
            }
            VirtualPath path = new VirtualPath();
            if (UrlPath.IsAppRelativePath(virtualPath))
            {
                if (flag2)
                {
                    virtualPath = UrlPath.ReduceVirtualPath(virtualPath);
                }
                if (virtualPath[0] == '~')
                {
                    if ((options & VirtualPathOptions.AllowAppRelativePath) == 0)
                    {
                        throw new ArgumentException(System.Web.SR.GetString("VirtualPath_AllowAppRelativePath", new object[] { virtualPath }));
                    }
                    path._appRelativeVirtualPath = virtualPath;
                    return path;
                }
                if ((options & VirtualPathOptions.AllowAbsolutePath) == 0)
                {
                    throw new ArgumentException(System.Web.SR.GetString("VirtualPath_AllowAbsolutePath", new object[] { virtualPath }));
                }
                path._virtualPath = virtualPath;
                return path;
            }
            if (virtualPath[0] != '/')
            {
                if ((options & VirtualPathOptions.AllowRelativePath) == 0)
                {
                    throw new ArgumentException(System.Web.SR.GetString("VirtualPath_AllowRelativePath", new object[] { virtualPath }));
                }
                path._virtualPath = virtualPath;
                return path;
            }
            if ((options & VirtualPathOptions.AllowAbsolutePath) == 0)
            {
                throw new ArgumentException(System.Web.SR.GetString("VirtualPath_AllowAbsolutePath", new object[] { virtualPath }));
            }
            if (flag2)
            {
                virtualPath = UrlPath.ReduceVirtualPath(virtualPath);
            }
            path._virtualPath = virtualPath;
            return path;
        }

        public static VirtualPath CreateAbsolute(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAbsolutePath);
        }

        public static VirtualPath CreateAbsoluteAllowNull(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.AllowNull);
        }

        public static VirtualPath CreateAbsoluteTrailingSlash(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.EnsureTrailingSlash);
        }

        public static VirtualPath CreateAllowNull(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAllPath | VirtualPathOptions.AllowNull);
        }

        public static VirtualPath CreateNonRelative(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAppRelativePath | VirtualPathOptions.AllowAbsolutePath);
        }

        public static VirtualPath CreateNonRelativeAllowNull(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAppRelativePath | VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.AllowNull);
        }

        public static VirtualPath CreateNonRelativeTrailingSlash(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAppRelativePath | VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.EnsureTrailingSlash);
        }

        public static VirtualPath CreateNonRelativeTrailingSlashAllowNull(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAppRelativePath | VirtualPathOptions.AllowAbsolutePath | VirtualPathOptions.EnsureTrailingSlash | VirtualPathOptions.AllowNull);
        }

        public static VirtualPath CreateTrailingSlash(string virtualPath)
        {
            return Create(virtualPath, VirtualPathOptions.AllowAllPath | VirtualPathOptions.EnsureTrailingSlash);
        }

        public bool DirectoryExists()
        {
            return HostingEnvironment.VirtualPathProvider.DirectoryExists(this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }
            VirtualPath path = value as VirtualPath;
            if (path == null)
            {
                return false;
            }
            return EqualsHelper(path, this);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool Equals(VirtualPath v1, VirtualPath v2)
        {
            return ((v1 == v2) || (((v1 != null) && (v2 != null)) && EqualsHelper(v1, v2)));
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        private static bool EqualsHelper(VirtualPath v1, VirtualPath v2)
        {
            return (StringComparer.InvariantCultureIgnoreCase.Compare(v1.VirtualPathString, v2.VirtualPathString) == 0);
        }

        internal void FailIfNotWithinAppRoot()
        {
            if (!this.IsWithinAppRoot)
            {
                throw new ArgumentException(System.Web.SR.GetString("Cross_app_not_allowed", new object[] { this.VirtualPathString }));
            }
        }

        internal void FailIfRelativePath()
        {
            if (this.IsRelative)
            {
                throw new ArgumentException(System.Web.SR.GetString("VirtualPath_AllowRelativePath", new object[] { this._virtualPath }));
            }
        }

        public bool FileExists()
        {
            return HostingEnvironment.VirtualPathProvider.FileExists(this);
        }

        internal static string GetAppRelativeVirtualPathString(VirtualPath virtualPath)
        {
            if (virtualPath != null)
            {
                return virtualPath.AppRelativeVirtualPathString;
            }
            return null;
        }

        internal static string GetAppRelativeVirtualPathStringOrEmpty(VirtualPath virtualPath)
        {
            if (virtualPath != null)
            {
                return virtualPath.AppRelativeVirtualPathString;
            }
            return string.Empty;
        }

        public CacheDependency GetCacheDependency(IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return HostingEnvironment.VirtualPathProvider.GetCacheDependency(this, virtualPathDependencies, utcStart);
        }

        public string GetCacheKey()
        {
            return HostingEnvironment.VirtualPathProvider.GetCacheKey(this);
        }

        public VirtualDirectory GetDirectory()
        {
            return HostingEnvironment.VirtualPathProvider.GetDirectory(this);
        }

        public VirtualFile GetFile()
        {
            return HostingEnvironment.VirtualPathProvider.GetFile(this);
        }

        public string GetFileHash(IEnumerable virtualPathDependencies)
        {
            return HostingEnvironment.VirtualPathProvider.GetFileHash(this, virtualPathDependencies);
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.VirtualPathString);
        }

        internal static string GetVirtualPathString(VirtualPath virtualPath)
        {
            if (virtualPath != null)
            {
                return virtualPath.VirtualPathString;
            }
            return null;
        }

        internal static string GetVirtualPathStringNoTrailingSlash(VirtualPath virtualPath)
        {
            if (virtualPath != null)
            {
                return virtualPath.VirtualPathStringNoTrailingSlash;
            }
            return null;
        }

        public VirtualPath MakeRelative(VirtualPath toVirtualPath)
        {
            VirtualPath path = new VirtualPath();
            this.FailIfRelativePath();
            toVirtualPath.FailIfRelativePath();
            path._virtualPath = UrlPath.MakeRelative(this.VirtualPathString, toVirtualPath.VirtualPathString);
            return path;
        }

        public string MapPath()
        {
            return HostingEnvironment.MapPath(this);
        }

        internal string MapPathInternal()
        {
            return HostingEnvironment.MapPathInternal(this);
        }

        internal string MapPathInternal(bool permitNull)
        {
            return HostingEnvironment.MapPathInternal(this, permitNull);
        }

        internal string MapPathInternal(VirtualPath baseVirtualDir, bool allowCrossAppMapping)
        {
            return HostingEnvironment.MapPathInternal(this, baseVirtualDir, allowCrossAppMapping);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(VirtualPath v1, VirtualPath v2)
        {
            return Equals(v1, v2);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(VirtualPath v1, VirtualPath v2)
        {
            return !Equals(v1, v2);
        }

        public Stream OpenFile()
        {
            return VirtualPathProvider.OpenFile(this);
        }

        internal VirtualPath SimpleCombine(string relativePath)
        {
            return this.SimpleCombine(relativePath, false);
        }

        private VirtualPath SimpleCombine(string filename, bool addTrailingSlash)
        {
            string virtualPath = this.VirtualPathStringWhicheverAvailable + filename;
            if (addTrailingSlash)
            {
                virtualPath = virtualPath + "/";
            }
            VirtualPath path = new VirtualPath(virtualPath);
            path.CopyFlagsFrom(this, 7);
            return path;
        }

        internal VirtualPath SimpleCombineWithDir(string directoryName)
        {
            return this.SimpleCombine(directoryName, true);
        }

        int IComparable.CompareTo(object obj)
        {
            VirtualPath path = obj as VirtualPath;
            if (path == null)
            {
                throw new ArgumentException();
            }
            if (path == this)
            {
                return 0;
            }
            return StringComparer.InvariantCultureIgnoreCase.Compare(this.VirtualPathString, path.VirtualPathString);
        }

        public override string ToString()
        {
            if ((this._virtualPath == null) && (HttpRuntime.AppDomainAppVirtualPathObject == null))
            {
                return this._appRelativeVirtualPath;
            }
            return this.VirtualPathString;
        }

        public string AppRelativeVirtualPathString
        {
            get
            {
                string appRelativeVirtualPathStringOrNull = this.AppRelativeVirtualPathStringOrNull;
                if (appRelativeVirtualPathStringOrNull == null)
                {
                    return this._virtualPath;
                }
                return appRelativeVirtualPathStringOrNull;
            }
        }

        internal string AppRelativeVirtualPathStringIfAvailable
        {
            get
            {
                return this._appRelativeVirtualPath;
            }
        }

        internal string AppRelativeVirtualPathStringOrNull
        {
            get
            {
                if (this._appRelativeVirtualPath == null)
                {
                    if (this.flags[4])
                    {
                        return null;
                    }
                    if (HttpRuntime.AppDomainAppVirtualPathObject == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("VirtualPath_CantMakeAppRelative", new object[] { this._virtualPath }));
                    }
                    this._appRelativeVirtualPath = UrlPath.MakeVirtualPathAppRelativeOrNull(this._virtualPath);
                    this.flags[4] = true;
                    if (this._appRelativeVirtualPath == null)
                    {
                        return null;
                    }
                }
                return this._appRelativeVirtualPath;
            }
        }

        public string Extension
        {
            get
            {
                return UrlPath.GetExtension(this.VirtualPathString);
            }
        }

        public string FileName
        {
            get
            {
                return UrlPath.GetFileName(this.VirtualPathStringNoTrailingSlash);
            }
        }

        internal bool HasTrailingSlash
        {
            get
            {
                if (this._virtualPath != null)
                {
                    return UrlPath.HasTrailingSlash(this._virtualPath);
                }
                return UrlPath.HasTrailingSlash(this._appRelativeVirtualPath);
            }
        }

        public bool IsRelative
        {
            get
            {
                return ((this._virtualPath != null) && (this._virtualPath[0] != '/'));
            }
        }

        public bool IsRoot
        {
            get
            {
                return (this._virtualPath == "/");
            }
        }

        public bool IsWithinAppRoot
        {
            get
            {
                if (!this.flags[1])
                {
                    if (HttpRuntime.AppDomainIdInternal == null)
                    {
                        return true;
                    }
                    if (this.flags[4])
                    {
                        this.flags[2] = this._appRelativeVirtualPath != null;
                    }
                    else
                    {
                        this.flags[2] = UrlPath.IsEqualOrSubpath(HttpRuntime.AppDomainAppVirtualPathString, this.VirtualPathString);
                    }
                    this.flags[1] = true;
                }
                return this.flags[2];
            }
        }

        public VirtualPath Parent
        {
            get
            {
                this.FailIfRelativePath();
                if (this.IsRoot)
                {
                    return null;
                }
                string virtualPathStringNoTrailingSlash = UrlPath.RemoveSlashFromPathIfNeeded(this.VirtualPathStringWhicheverAvailable);
                if (virtualPathStringNoTrailingSlash == "~")
                {
                    virtualPathStringNoTrailingSlash = this.VirtualPathStringNoTrailingSlash;
                }
                int num = virtualPathStringNoTrailingSlash.LastIndexOf('/');
                if (num == 0)
                {
                    return RootVirtualPath;
                }
                return new VirtualPath(virtualPathStringNoTrailingSlash.Substring(0, num + 1));
            }
        }

        public string VirtualPathString
        {
            get
            {
                if (this._virtualPath == null)
                {
                    if (HttpRuntime.AppDomainAppVirtualPathObject == null)
                    {
                        throw new HttpException(System.Web.SR.GetString("VirtualPath_CantMakeAppAbsolute", new object[] { this._appRelativeVirtualPath }));
                    }
                    if (this._appRelativeVirtualPath.Length == 1)
                    {
                        this._virtualPath = HttpRuntime.AppDomainAppVirtualPath;
                    }
                    else
                    {
                        this._virtualPath = HttpRuntime.AppDomainAppVirtualPathString + this._appRelativeVirtualPath.Substring(2);
                    }
                }
                return this._virtualPath;
            }
        }

        internal string VirtualPathStringIfAvailable
        {
            get
            {
                return this._virtualPath;
            }
        }

        internal string VirtualPathStringNoTrailingSlash
        {
            get
            {
                return UrlPath.RemoveSlashFromPathIfNeeded(this.VirtualPathString);
            }
        }

        internal string VirtualPathStringWhicheverAvailable
        {
            get
            {
                if (this._virtualPath == null)
                {
                    return this._appRelativeVirtualPath;
                }
                return this._virtualPath;
            }
        }
    }
}

