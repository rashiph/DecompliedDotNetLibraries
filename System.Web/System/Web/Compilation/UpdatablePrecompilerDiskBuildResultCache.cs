namespace System.Web.Compilation
{
    using System;

    internal class UpdatablePrecompilerDiskBuildResultCache : PrecompilerDiskBuildResultCache
    {
        internal UpdatablePrecompilerDiskBuildResultCache(string cacheDir) : base(cacheDir)
        {
        }

        internal override void CacheBuildResult(string cacheKey, BuildResult result, long hashCode, DateTime utcStart)
        {
            if (!(result is BuildResultCompiledTemplateType))
            {
                base.CacheBuildResult(cacheKey, result, hashCode, utcStart);
            }
        }
    }
}

