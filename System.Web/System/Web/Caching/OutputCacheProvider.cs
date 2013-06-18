namespace System.Web.Caching
{
    using System;
    using System.Configuration.Provider;

    public abstract class OutputCacheProvider : ProviderBase
    {
        protected OutputCacheProvider()
        {
        }

        public abstract object Add(string key, object entry, DateTime utcExpiry);
        public abstract object Get(string key);
        public abstract void Remove(string key);
        public abstract void Set(string key, object entry, DateTime utcExpiry);
    }
}

