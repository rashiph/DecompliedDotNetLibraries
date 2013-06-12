namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    internal sealed class OverlappedDataCache : CriticalFinalizerObject
    {
        private const float m_CleanupInitialThreadhold = 0.3f;
        private static int m_cleanupObjectCount;
        private const float m_CleanupStep = 0.05f;
        private static float m_CleanupThreshold;
        private int m_gen2GCCount;
        private static OverlappedDataCacheLine m_overlappedDataCache;
        private static int m_overlappedDataCacheAccessed;
        private bool m_ready;
        private static volatile OverlappedDataCacheLine s_firstFreeCacheLine = null;

        internal OverlappedDataCache()
        {
            if (m_cleanupObjectCount == 0)
            {
                m_CleanupThreshold = 0.3f;
                if (Interlocked.Exchange(ref m_cleanupObjectCount, 1) == 0)
                {
                    this.m_ready = true;
                }
            }
        }

        [SecurityCritical]
        internal static void CacheOverlappedData(OverlappedData data)
        {
            data.ReInitialize();
            data.m_cacheLine.m_items[data.m_slot] = data;
            s_firstFreeCacheLine = null;
        }

        [SecuritySafeCritical]
        ~OverlappedDataCache()
        {
            if (this.m_ready)
            {
                if (m_overlappedDataCache == null)
                {
                    Interlocked.Exchange(ref m_cleanupObjectCount, 0);
                }
                else
                {
                    if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        GC.ReRegisterForFinalize(this);
                    }
                    int num = GC.CollectionCount(GC.MaxGeneration);
                    if (num != this.m_gen2GCCount)
                    {
                        this.m_gen2GCCount = num;
                        OverlappedDataCacheLine line = null;
                        OverlappedDataCacheLine overlappedDataCache = m_overlappedDataCache;
                        OverlappedDataCacheLine line3 = null;
                        OverlappedDataCacheLine line4 = line;
                        int num2 = 0;
                        int num3 = 0;
                        while (overlappedDataCache != null)
                        {
                            num2++;
                            bool flag = false;
                            for (short i = 0; i < 0x10; i = (short) (i + 1))
                            {
                                if (overlappedDataCache.m_items[i] == null)
                                {
                                    flag = true;
                                    num3++;
                                }
                            }
                            if (!flag)
                            {
                                line4 = line;
                                line3 = overlappedDataCache;
                            }
                            line = overlappedDataCache;
                            overlappedDataCache = overlappedDataCache.m_next;
                        }
                        num2 *= 0x10;
                        if ((line3 != null) && ((num2 * m_CleanupThreshold) > num3))
                        {
                            if (line4 == null)
                            {
                                m_overlappedDataCache = line3.m_next;
                            }
                            else
                            {
                                line4.m_next = line3.m_next;
                            }
                            line3.Removed = true;
                        }
                        if (m_overlappedDataCacheAccessed != 0)
                        {
                            m_CleanupThreshold = 0.3f;
                            Interlocked.Exchange(ref m_overlappedDataCacheAccessed, 0);
                        }
                        else
                        {
                            m_CleanupThreshold += 0.05f;
                        }
                    }
                }
            }
        }

        internal static OverlappedData GetOverlappedData(Overlapped overlapped)
        {
            OverlappedData data = null;
            Interlocked.Exchange(ref m_overlappedDataCacheAccessed, 1);
            while (true)
            {
                OverlappedDataCacheLine overlappedDataCache = s_firstFreeCacheLine;
                if (overlappedDataCache == null)
                {
                    overlappedDataCache = m_overlappedDataCache;
                }
                while (overlappedDataCache != null)
                {
                    for (short i = 0; i < 0x10; i = (short) (i + 1))
                    {
                        if (overlappedDataCache.m_items[i] != null)
                        {
                            data = Interlocked.Exchange<OverlappedData>(ref overlappedDataCache.m_items[i], null);
                            if (data != null)
                            {
                                s_firstFreeCacheLine = overlappedDataCache;
                                data.m_overlapped = overlapped;
                                return data;
                            }
                        }
                    }
                    overlappedDataCache = overlappedDataCache.m_next;
                }
                GrowOverlappedDataCache();
            }
        }

        private static void GrowOverlappedDataCache()
        {
            OverlappedDataCacheLine line = new OverlappedDataCacheLine();
            if ((m_overlappedDataCache == null) && (Interlocked.CompareExchange<OverlappedDataCacheLine>(ref m_overlappedDataCache, line, null) == null))
            {
                new OverlappedDataCache();
            }
            else
            {
                OverlappedDataCacheLine line2;
                if (m_cleanupObjectCount == 0)
                {
                    new OverlappedDataCache();
                }
                do
                {
                    for (line2 = m_overlappedDataCache; (line2 != null) && (line2.m_next != null); line2 = line2.m_next)
                    {
                    }
                }
                while ((line2 != null) && (Interlocked.CompareExchange<OverlappedDataCacheLine>(ref line2.m_next, line, null) != null));
            }
        }
    }
}

