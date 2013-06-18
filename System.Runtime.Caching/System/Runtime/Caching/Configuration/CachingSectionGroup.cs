namespace System.Runtime.Caching.Configuration
{
    using System.Configuration;

    public sealed class CachingSectionGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("memoryCache")]
        public MemoryCacheSection MemoryCaches
        {
            get
            {
                return (MemoryCacheSection) base.Sections["memoryCache"];
            }
        }
    }
}

