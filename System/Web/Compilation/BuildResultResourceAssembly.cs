namespace System.Web.Compilation
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    internal class BuildResultResourceAssembly : BuildResultCompiledAssembly
    {
        private string _resourcesDependenciesHash;

        internal BuildResultResourceAssembly()
        {
        }

        internal BuildResultResourceAssembly(Assembly a) : base(a)
        {
        }

        internal override string ComputeSourceDependenciesHashCode(VirtualPath virtualPath)
        {
            if (virtualPath == null)
            {
                virtualPath = base.VirtualPath;
            }
            HashCodeCombiner combiner = new HashCodeCombiner();
            combiner.AddResourcesDirectory(virtualPath.MapPathInternal());
            return combiner.CombinedHashString;
        }

        private void EnsureResourcesDependenciesHashComputed()
        {
            if (this._resourcesDependenciesHash == null)
            {
                this._resourcesDependenciesHash = HashCodeCombiner.GetDirectoryHash(base.VirtualPath);
            }
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.BuildResultResourceAssembly;
        }

        internal override void GetPreservedAttributes(PreservationFileReader pfr)
        {
            base.GetPreservedAttributes(pfr);
            this.ResourcesDependenciesHash = pfr.GetAttribute("resHash");
        }

        internal override void SetPreservedAttributes(PreservationFileWriter pfw)
        {
            base.SetPreservedAttributes(pfw);
            pfw.SetAttribute("resHash", this.ResourcesDependenciesHash);
        }

        internal string ResourcesDependenciesHash
        {
            get
            {
                this.EnsureResourcesDependenciesHashComputed();
                return this._resourcesDependenciesHash;
            }
            set
            {
                this._resourcesDependenciesHash = value;
            }
        }
    }
}

