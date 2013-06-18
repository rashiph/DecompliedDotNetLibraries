namespace System.Runtime.Caching
{
    using System;
    using System.Runtime.CompilerServices;

    public class CacheItem
    {
        private CacheItem()
        {
        }

        public CacheItem(string key)
        {
            this.Key = key;
        }

        public CacheItem(string key, object value) : this(key)
        {
            this.Value = value;
        }

        public CacheItem(string key, object value, string regionName) : this(key, value)
        {
            this.RegionName = regionName;
        }

        public string Key { get; set; }

        public string RegionName { get; set; }

        public object Value { get; set; }
    }
}

