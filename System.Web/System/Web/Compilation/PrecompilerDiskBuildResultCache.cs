namespace System.Web.Compilation
{
    using System;

    internal class PrecompilerDiskBuildResultCache : PrecompBaseDiskBuildResultCache
    {
        internal PrecompilerDiskBuildResultCache(string cacheDir) : base(cacheDir)
        {
            base.EnsureDiskCacheDirectoryCreated();
        }
    }
}

