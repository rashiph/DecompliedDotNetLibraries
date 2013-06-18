namespace System.Web.Caching
{
    using System;

    internal class DependencyCacheEntry
    {
        private string _kernelCacheEntryKey;
        private string _outputCacheEntryKey;
        private string _providerName;

        internal DependencyCacheEntry(string oceKey, string kernelCacheEntryKey, string providerName)
        {
            this._outputCacheEntryKey = oceKey;
            this._kernelCacheEntryKey = kernelCacheEntryKey;
            this._providerName = providerName;
        }

        internal string KernelCacheEntryKey
        {
            get
            {
                return this._kernelCacheEntryKey;
            }
        }

        internal string OutputCacheEntryKey
        {
            get
            {
                return this._outputCacheEntryKey;
            }
        }

        internal string ProviderName
        {
            get
            {
                return this._providerName;
            }
        }
    }
}

