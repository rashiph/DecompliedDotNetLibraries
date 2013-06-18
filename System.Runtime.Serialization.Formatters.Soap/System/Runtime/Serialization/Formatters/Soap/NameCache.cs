namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;

    internal sealed class NameCache
    {
        private const int MAX_CACHE_ENTRIES = 0x161;
        private string name;
        private static NameCacheEntry[] nameCache = new NameCacheEntry[0x161];
        private int probe;

        internal object GetCachedValue(string name)
        {
            this.name = name;
            this.probe = Math.Abs(name.GetHashCode()) % 0x161;
            NameCacheEntry entry = nameCache[this.probe];
            if (entry == null)
            {
                entry = new NameCacheEntry {
                    name = name
                };
                return null;
            }
            if (entry.name == name)
            {
                return entry.value;
            }
            return null;
        }

        internal void SetCachedValue(object value)
        {
            nameCache[this.probe] = new NameCacheEntry { name = this.name, value = value };
        }
    }
}

