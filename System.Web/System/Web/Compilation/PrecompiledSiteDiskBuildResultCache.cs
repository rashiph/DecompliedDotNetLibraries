namespace System.Web.Compilation
{
    using System;

    internal class PrecompiledSiteDiskBuildResultCache : PrecompBaseDiskBuildResultCache
    {
        internal PrecompiledSiteDiskBuildResultCache(string cacheDir) : base(cacheDir)
        {
        }

        internal override void CacheBuildResult(string cacheKey, BuildResult result, long hashCode, DateTime utcStart)
        {
        }

        internal override void RemoveAssemblyAndRelatedFiles(string baseName)
        {
        }

        protected override bool PrecompilationMode
        {
            get
            {
                return true;
            }
        }
    }
}

