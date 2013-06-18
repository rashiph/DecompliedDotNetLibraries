namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;

    internal abstract class BuildResult
    {
        protected SimpleBitVector32 _flags;
        private int _lock;
        private DateTime _nextUpToDateCheck = DateTime.Now.AddSeconds(2.0);
        private System.Web.VirtualPath _virtualPath;
        private ArrayList _virtualPathDependencies;
        private string _virtualPathDependenciesHash;
        protected const int dependenciesHashComputed = 0x100000;
        protected const int hasAppOrSessionObjects = 0x80000;
        private const int noMemoryCache = 0x40000;
        private const int UpdateInterval = 2;
        protected const int usesCacheDependency = 0x10000;
        protected const int usesExistingAssembly = 0x20000;

        protected BuildResult()
        {
        }

        internal void AddVirtualPathDependencies(ICollection sourceDependencies)
        {
            if (sourceDependencies != null)
            {
                if (this._virtualPathDependencies == null)
                {
                    this._virtualPathDependencies = new ArrayList(sourceDependencies);
                }
                else
                {
                    this._virtualPathDependencies.AddRange(sourceDependencies);
                }
            }
        }

        internal long ComputeHashCode(long hashCode)
        {
            return this.ComputeHashCode(hashCode, 0L);
        }

        protected virtual void ComputeHashCode(HashCodeCombiner hashCodeCombiner)
        {
        }

        internal long ComputeHashCode(long hashCode1, long hashCode2)
        {
            HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
            if (hashCode1 != 0L)
            {
                hashCodeCombiner.AddObject(hashCode1);
            }
            if (hashCode2 != 0L)
            {
                hashCodeCombiner.AddObject(hashCode2);
            }
            this.ComputeHashCode(hashCodeCombiner);
            return hashCodeCombiner.CombinedHash;
        }

        internal virtual string ComputeSourceDependenciesHashCode(System.Web.VirtualPath virtualPath)
        {
            if (this.VirtualPathDependencies == null)
            {
                return string.Empty;
            }
            if (virtualPath == null)
            {
                virtualPath = this.VirtualPath;
            }
            return virtualPath.GetFileHash(this.VirtualPathDependencies);
        }

        internal static BuildResult CreateBuildResultFromCode(BuildResultTypeCode code, System.Web.VirtualPath virtualPath)
        {
            BuildResult result = null;
            switch (code)
            {
                case BuildResultTypeCode.BuildResultCompiledAssembly:
                    result = new BuildResultCompiledAssembly();
                    break;

                case BuildResultTypeCode.BuildResultCompiledType:
                    result = new BuildResultCompiledType();
                    break;

                case BuildResultTypeCode.BuildResultCompiledTemplateType:
                    result = new BuildResultCompiledTemplateType();
                    break;

                case BuildResultTypeCode.BuildResultCustomString:
                    result = new BuildResultCustomString();
                    break;

                case BuildResultTypeCode.BuildResultMainCodeAssembly:
                    result = new BuildResultMainCodeAssembly();
                    break;

                case BuildResultTypeCode.BuildResultCodeCompileUnit:
                    result = new BuildResultCodeCompileUnit();
                    break;

                case BuildResultTypeCode.BuildResultCompiledGlobalAsaxType:
                    result = new BuildResultCompiledGlobalAsaxType();
                    break;

                case BuildResultTypeCode.BuildResultResourceAssembly:
                    result = new BuildResultResourceAssembly();
                    break;

                default:
                    return null;
            }
            result.VirtualPath = virtualPath;
            result._nextUpToDateCheck = DateTime.MinValue;
            return result;
        }

        internal void EnsureVirtualPathDependenciesHashComputed()
        {
            if (!this.DependenciesHashComputed)
            {
                if (this._virtualPathDependencies != null)
                {
                    this._virtualPathDependencies.Sort(System.InvariantComparer.Default);
                }
                this._virtualPathDependenciesHash = this.ComputeSourceDependenciesHashCode(null);
                this._flags[0x100000] = true;
            }
        }

        internal virtual BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.Invalid;
        }

        internal virtual void GetPreservedAttributes(PreservationFileReader pfr)
        {
            this.ReadPreservedFlags(pfr);
        }

        internal bool IsUpToDate(System.Web.VirtualPath virtualPath, bool ensureIsUpToDate)
        {
            if (ensureIsUpToDate)
            {
                string str;
                if (this._lock < 0)
                {
                    return false;
                }
                DateTime now = DateTime.Now;
                if ((now < this._nextUpToDateCheck) && !BuildManagerHost.InClientBuildManager)
                {
                    return true;
                }
                if (Interlocked.CompareExchange(ref this._lock, 1, 0) != 0)
                {
                    return true;
                }
                try
                {
                    str = this.ComputeSourceDependenciesHashCode(virtualPath);
                }
                catch
                {
                    Interlocked.Exchange(ref this._lock, 0);
                    throw;
                }
                if ((str == null) || (str != this._virtualPathDependenciesHash))
                {
                    this._lock = -1;
                    return false;
                }
                this._nextUpToDateCheck = now.AddSeconds(2.0);
                Interlocked.Exchange(ref this._lock, 0);
            }
            return true;
        }

        protected void ReadPreservedFlags(PreservationFileReader pfr)
        {
            string attribute = pfr.GetAttribute("flags");
            if ((attribute != null) && (attribute.Length != 0))
            {
                this.Flags = int.Parse(attribute, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            }
        }

        internal virtual void RemoveOutOfDateResources(PreservationFileReader pfw)
        {
        }

        internal virtual void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            if (this.Flags != 0)
            {
                pfw.SetAttribute("flags", this.Flags.ToString("x", CultureInfo.InvariantCulture));
            }
        }

        internal void SetVirtualPathDependencies(ArrayList sourceDependencies)
        {
            this._virtualPathDependencies = sourceDependencies;
        }

        internal virtual bool CacheToDisk
        {
            get
            {
                return true;
            }
        }

        internal bool CacheToMemory
        {
            get
            {
                return !this._flags[0x40000];
            }
            set
            {
                this._flags[0x40000] = !value;
            }
        }

        internal bool DependenciesHashComputed
        {
            get
            {
                return this._flags[0x100000];
            }
        }

        internal int Flags
        {
            get
            {
                return this._flags.IntegerValue;
            }
            set
            {
                this._flags.IntegerValue = value;
            }
        }

        internal virtual bool IsUnloadable
        {
            get
            {
                return true;
            }
        }

        internal virtual DateTime MemoryCacheExpiration
        {
            get
            {
                return Cache.NoAbsoluteExpiration;
            }
        }

        internal virtual TimeSpan MemoryCacheSlidingExpiration
        {
            get
            {
                return Cache.NoSlidingExpiration;
            }
        }

        internal bool ShutdownAppDomainOnChange
        {
            get
            {
                return this._flags[1];
            }
        }

        internal bool UsesCacheDependency
        {
            get
            {
                return this._flags[0x10000];
            }
            set
            {
                this._flags[0x10000] = value;
            }
        }

        internal System.Web.VirtualPath VirtualPath
        {
            get
            {
                return this._virtualPath;
            }
            set
            {
                this._virtualPath = value;
            }
        }

        internal ICollection VirtualPathDependencies
        {
            get
            {
                return this._virtualPathDependencies;
            }
        }

        internal string VirtualPathDependenciesHash
        {
            get
            {
                this.EnsureVirtualPathDependenciesHashComputed();
                return this._virtualPathDependenciesHash;
            }
            set
            {
                this._virtualPathDependenciesHash = value;
            }
        }
    }
}

