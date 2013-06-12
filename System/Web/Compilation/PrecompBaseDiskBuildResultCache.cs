namespace System.Web.Compilation
{
    using System;

    internal abstract class PrecompBaseDiskBuildResultCache : DiskBuildResultCache
    {
        internal PrecompBaseDiskBuildResultCache(string cacheDir) : base(cacheDir)
        {
        }
    }
}

