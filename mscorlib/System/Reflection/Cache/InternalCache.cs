namespace System.Reflection.Cache
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Security;

    [Serializable]
    internal class InternalCache
    {
        private InternalCacheItem[] m_cache;
        private int m_numItems;
        private const int MinCacheSize = 2;

        internal InternalCache(string cacheName)
        {
        }

        private int FindObjectPosition(InternalCacheItem[] cache, int itemCount, CacheObjType cacheType, bool findEmpty)
        {
            if (cache != null)
            {
                if (itemCount > cache.Length)
                {
                    itemCount = cache.Length;
                }
                for (int i = 0; i < itemCount; i++)
                {
                    if (cacheType == cache[i].Key)
                    {
                        return i;
                    }
                }
                if (findEmpty && (itemCount < (cache.Length - 1)))
                {
                    return (itemCount + 1);
                }
            }
            return -1;
        }

        [Conditional("_LOGGING")]
        private void LogAction(CacheAction action, CacheObjType cacheType)
        {
        }

        [Conditional("_LOGGING")]
        private void LogAction(CacheAction action, CacheObjType cacheType, object obj)
        {
        }

        internal object this[CacheObjType cacheType]
        {
            [SecurityCritical]
            get
            {
                InternalCacheItem[] cache = this.m_cache;
                int numItems = this.m_numItems;
                int index = this.FindObjectPosition(cache, numItems, cacheType, false);
                if (index >= 0)
                {
                    bool flag1 = BCLDebug.m_loggingNotEnabled;
                    return cache[index].Value;
                }
                bool loggingNotEnabled = BCLDebug.m_loggingNotEnabled;
                return null;
            }
            [SecurityCritical]
            set
            {
                bool loggingNotEnabled = BCLDebug.m_loggingNotEnabled;
                lock (this)
                {
                    int index = this.FindObjectPosition(this.m_cache, this.m_numItems, cacheType, true);
                    if (index > 0)
                    {
                        this.m_cache[index].Value = value;
                        this.m_cache[index].Key = cacheType;
                        if (index == this.m_numItems)
                        {
                            this.m_numItems++;
                        }
                    }
                    else if (this.m_cache == null)
                    {
                        bool flag2 = BCLDebug.m_loggingNotEnabled;
                        this.m_cache = new InternalCacheItem[2];
                        this.m_cache[0].Value = value;
                        this.m_cache[0].Key = cacheType;
                        this.m_numItems = 1;
                    }
                    else
                    {
                        bool flag3 = BCLDebug.m_loggingNotEnabled;
                        InternalCacheItem[] itemArray = new InternalCacheItem[this.m_numItems * 2];
                        for (int i = 0; i < this.m_numItems; i++)
                        {
                            itemArray[i] = this.m_cache[i];
                        }
                        itemArray[this.m_numItems].Value = value;
                        itemArray[this.m_numItems].Key = cacheType;
                        this.m_cache = itemArray;
                        this.m_numItems++;
                    }
                }
            }
        }
    }
}

