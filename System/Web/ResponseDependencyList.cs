namespace System.Web
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.Util;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ResponseDependencyList
    {
        private ArrayList _dependencies;
        private string[] _dependencyArray;
        private DateTime _oldestDependency;
        private string _requestVirtualPath;
        internal void AddDependency(string item, string argname)
        {
            if (item == null)
            {
                throw new ArgumentNullException(argname);
            }
            this._dependencyArray = null;
            if (this._dependencies == null)
            {
                this._dependencies = new ArrayList(1);
            }
            DateTime utcNow = DateTime.UtcNow;
            this._dependencies.Add(new ResponseDependencyInfo(new string[] { item }, utcNow));
            if ((this._oldestDependency == DateTime.MinValue) || (utcNow < this._oldestDependency))
            {
                this._oldestDependency = utcNow;
            }
        }

        internal void AddDependencies(ArrayList items, string argname)
        {
            if (items == null)
            {
                throw new ArgumentNullException(argname);
            }
            string[] strArray = (string[]) items.ToArray(typeof(string));
            this.AddDependencies(strArray, argname, false);
        }

        internal void AddDependencies(string[] items, string argname)
        {
            this.AddDependencies(items, argname, true);
        }

        internal void AddDependencies(string[] items, string argname, bool cloneArray)
        {
            this.AddDependencies(items, argname, cloneArray, DateTime.UtcNow);
        }

        internal void AddDependencies(string[] items, string argname, bool cloneArray, string requestVirtualPath)
        {
            if (requestVirtualPath == null)
            {
                throw new ArgumentNullException("requestVirtualPath");
            }
            this._requestVirtualPath = requestVirtualPath;
            this.AddDependencies(items, argname, cloneArray, DateTime.UtcNow);
        }

        internal void AddDependencies(string[] items, string argname, bool cloneArray, DateTime utcDepTime)
        {
            string[] strArray;
            if (items == null)
            {
                throw new ArgumentNullException(argname);
            }
            if (cloneArray)
            {
                strArray = (string[]) items.Clone();
            }
            else
            {
                strArray = items;
            }
            foreach (string str in strArray)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentNullException(argname);
                }
            }
            this._dependencyArray = null;
            if (this._dependencies == null)
            {
                this._dependencies = new ArrayList(1);
            }
            this._dependencies.Add(new ResponseDependencyInfo(strArray, utcDepTime));
            if ((this._oldestDependency == DateTime.MinValue) || (utcDepTime < this._oldestDependency))
            {
                this._oldestDependency = utcDepTime;
            }
        }

        internal bool HasDependencies()
        {
            if ((this._dependencyArray == null) && (this._dependencies == null))
            {
                return false;
            }
            return true;
        }

        internal string[] GetDependencies()
        {
            if ((this._dependencyArray == null) && (this._dependencies != null))
            {
                int num = 0;
                foreach (ResponseDependencyInfo info in this._dependencies)
                {
                    num += info.items.Length;
                }
                this._dependencyArray = new string[num];
                int destinationIndex = 0;
                foreach (ResponseDependencyInfo info2 in this._dependencies)
                {
                    int length = info2.items.Length;
                    Array.Copy(info2.items, 0, this._dependencyArray, destinationIndex, length);
                    destinationIndex += length;
                }
            }
            return this._dependencyArray;
        }

        internal CacheDependency CreateCacheDependency(CacheDependencyType dependencyType, CacheDependency dependency)
        {
            if (this._dependencies != null)
            {
                if ((dependencyType == CacheDependencyType.Files) || (dependencyType == CacheDependencyType.CacheItems))
                {
                    foreach (ResponseDependencyInfo info in this._dependencies)
                    {
                        using (CacheDependency dependency2 = dependency)
                        {
                            if (dependencyType == CacheDependencyType.Files)
                            {
                                dependency = new CacheDependency(0, info.items, null, dependency2, info.utcDate);
                            }
                            else
                            {
                                dependency = new CacheDependency(null, info.items, dependency2, DateTimeUtil.ConvertToLocalTime(info.utcDate));
                            }
                        }
                    }
                    return dependency;
                }
                CacheDependency dependency3 = null;
                VirtualPathProvider virtualPathProvider = HostingEnvironment.VirtualPathProvider;
                if ((virtualPathProvider != null) && (this._requestVirtualPath != null))
                {
                    dependency3 = virtualPathProvider.GetCacheDependency(this._requestVirtualPath, this.GetDependencies(), this._oldestDependency);
                }
                if (dependency3 == null)
                {
                    return dependency;
                }
                AggregateCacheDependency dependency4 = new AggregateCacheDependency();
                dependency4.Add(new CacheDependency[] { dependency3 });
                if (dependency != null)
                {
                    dependency4.Add(new CacheDependency[] { dependency });
                }
                dependency = dependency4;
            }
            return dependency;
        }
    }
}

