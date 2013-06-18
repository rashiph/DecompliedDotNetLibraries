namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Web;

    public sealed class AggregateCacheDependency : CacheDependency, ICacheDependencyChanged
    {
        private ArrayList _dependencies;
        private bool _disposed;

        public AggregateCacheDependency()
        {
            base.FinishInit();
        }

        public void Add(params CacheDependency[] dependencies)
        {
            DateTime minValue = DateTime.MinValue;
            if (dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }
            dependencies = (CacheDependency[]) dependencies.Clone();
            foreach (CacheDependency dependency in dependencies)
            {
                if (dependency == null)
                {
                    throw new ArgumentNullException("dependencies");
                }
                if (!dependency.Use())
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("Cache_dependency_used_more_that_once"));
                }
            }
            bool flag = false;
            lock (this)
            {
                if (!this._disposed)
                {
                    if (this._dependencies == null)
                    {
                        this._dependencies = new ArrayList();
                    }
                    this._dependencies.AddRange(dependencies);
                    foreach (CacheDependency dependency2 in dependencies)
                    {
                        dependency2.SetCacheDependencyChanged(this);
                        if (dependency2.UtcLastModified > minValue)
                        {
                            minValue = dependency2.UtcLastModified;
                        }
                        if (dependency2.HasChanged)
                        {
                            flag = true;
                            goto Label_00EC;
                        }
                    }
                }
            }
        Label_00EC:
            base.SetUtcLastModified(minValue);
            if (flag)
            {
                base.NotifyDependencyChanged(this, EventArgs.Empty);
            }
        }

        protected override void DependencyDispose()
        {
            CacheDependency[] dependencyArray = null;
            lock (this)
            {
                this._disposed = true;
                if (this._dependencies != null)
                {
                    dependencyArray = (CacheDependency[]) this._dependencies.ToArray(typeof(CacheDependency));
                    this._dependencies = null;
                }
            }
            if (dependencyArray != null)
            {
                foreach (CacheDependency dependency in dependencyArray)
                {
                    dependency.DisposeInternal();
                }
            }
        }

        internal CacheDependency[] GetDependencyArray()
        {
            CacheDependency[] dependencyArray = null;
            lock (this)
            {
                if (this._dependencies != null)
                {
                    dependencyArray = (CacheDependency[]) this._dependencies.ToArray(typeof(CacheDependency));
                }
            }
            return dependencyArray;
        }

        internal override string[] GetFileDependencies()
        {
            ArrayList list = null;
            CacheDependency[] dependencyArray = null;
            dependencyArray = this.GetDependencyArray();
            if (dependencyArray != null)
            {
                foreach (CacheDependency dependency in dependencyArray)
                {
                    if (object.ReferenceEquals(dependency.GetType(), typeof(CacheDependency)) || object.ReferenceEquals(dependency.GetType(), typeof(AggregateCacheDependency)))
                    {
                        string[] fileDependencies = dependency.GetFileDependencies();
                        if (fileDependencies != null)
                        {
                            if (list == null)
                            {
                                list = new ArrayList();
                            }
                            list.AddRange(fileDependencies);
                        }
                    }
                }
                if (list != null)
                {
                    return (string[]) list.ToArray(typeof(string));
                }
            }
            return null;
        }

        public override string GetUniqueID()
        {
            StringBuilder builder = null;
            CacheDependency[] dependencyArray = null;
            if (this._dependencies == null)
            {
                return null;
            }
            lock (this)
            {
                if (this._dependencies != null)
                {
                    dependencyArray = (CacheDependency[]) this._dependencies.ToArray(typeof(CacheDependency));
                }
            }
            if (dependencyArray != null)
            {
                foreach (CacheDependency dependency in dependencyArray)
                {
                    string uniqueID = dependency.GetUniqueID();
                    if (uniqueID == null)
                    {
                        return null;
                    }
                    if (builder == null)
                    {
                        builder = new StringBuilder();
                    }
                    builder.Append(uniqueID);
                }
            }
            if (builder == null)
            {
                return null;
            }
            return builder.ToString();
        }

        internal override bool IsFileDependency()
        {
            CacheDependency[] dependencyArray = null;
            dependencyArray = this.GetDependencyArray();
            if (dependencyArray == null)
            {
                return false;
            }
            foreach (CacheDependency dependency in dependencyArray)
            {
                if (!object.ReferenceEquals(dependency.GetType(), typeof(CacheDependency)) && !object.ReferenceEquals(dependency.GetType(), typeof(AggregateCacheDependency)))
                {
                    return false;
                }
                if (!dependency.IsFileDependency())
                {
                    return false;
                }
            }
            return true;
        }

        void ICacheDependencyChanged.DependencyChanged(object sender, EventArgs e)
        {
            base.NotifyDependencyChanged(sender, e);
        }
    }
}

