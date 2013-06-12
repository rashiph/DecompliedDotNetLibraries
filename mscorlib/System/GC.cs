namespace System
{
    using System.Globalization;
    using System.Reflection.Cache;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public static class GC
    {
        private static readonly object locker = new object();
        private static ClearCacheHandler m_cacheHandler;

        internal static  event ClearCacheHandler ClearCache
        {
            [SecuritySafeCritical] add
            {
                lock (locker)
                {
                    m_cacheHandler = (ClearCacheHandler) Delegate.Combine(m_cacheHandler, value);
                    SetCleanupCache();
                }
            }
            remove
            {
                lock (locker)
                {
                    m_cacheHandler = (ClearCacheHandler) Delegate.Remove(m_cacheHandler, value);
                }
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _AddMemoryPressure(ulong bytesAllocated);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool _CancelFullGCNotification();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _Collect(int generation, int mode);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern int _CollectionCount(int generation);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool _RegisterForFullGCNotification(int maxGenerationPercentage, int largeObjectHeapPercentage);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _RemoveMemoryPressure(ulong bytesAllocated);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _ReRegisterForFinalize(object o);
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        private static extern void _SuppressFinalize(object o);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int _WaitForFullGCApproach(int millisecondsTimeout);
        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern int _WaitForFullGCComplete(int millisecondsTimeout);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _WaitForPendingFinalizers();
        [SecurityCritical]
        public static void AddMemoryPressure(long bytesAllocated)
        {
            if (bytesAllocated <= 0L)
            {
                throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if ((4 == IntPtr.Size) && (bytesAllocated > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("pressure", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
            }
            _AddMemoryPressure((ulong) bytesAllocated);
        }

        [SecurityCritical]
        public static void CancelFullGCNotification()
        {
            if (!_CancelFullGCNotification())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
            }
        }

        [SecuritySafeCritical]
        public static void Collect()
        {
            _Collect(-1, 0);
        }

        public static void Collect(int generation)
        {
            Collect(generation, GCCollectionMode.Default);
        }

        [SecuritySafeCritical]
        public static void Collect(int generation, GCCollectionMode mode)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((mode < GCCollectionMode.Default) || (mode > GCCollectionMode.Optimized))
            {
                throw new ArgumentOutOfRangeException(Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            _Collect(generation, (int) mode);
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int CollectionCount(int generation)
        {
            if (generation < 0)
            {
                throw new ArgumentOutOfRangeException("generation", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            return _CollectionCount(generation);
        }

        internal static void FireCacheEvent()
        {
            ClearCacheHandler handler = Interlocked.Exchange<ClearCacheHandler>(ref m_cacheHandler, null);
            if (handler != null)
            {
                handler(null, null);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern int GetGCLatencyMode();
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern int GetGeneration(object obj);
        [SecuritySafeCritical]
        public static int GetGeneration(WeakReference wo)
        {
            int generationWR = GetGenerationWR(wo.m_handle);
            KeepAlive(wo);
            return generationWR;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int GetGenerationWR(IntPtr handle);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int GetMaxGeneration();
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern long GetTotalMemory();
        [SecuritySafeCritical]
        public static long GetTotalMemory(bool forceFullCollection)
        {
            float num4;
            long totalMemory = GetTotalMemory();
            if (!forceFullCollection)
            {
                return totalMemory;
            }
            int num2 = 20;
            long num3 = totalMemory;
            do
            {
                WaitForPendingFinalizers();
                Collect();
                totalMemory = num3;
                num3 = GetTotalMemory();
                num4 = ((float) (num3 - totalMemory)) / ((float) totalMemory);
            }
            while ((num2-- > 0) && ((-0.05 >= num4) || (num4 >= 0.05)));
            return num3;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsServerGC();
        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
        public static extern void KeepAlive(object obj);
        [SecurityCritical]
        public static void RegisterForFullGCNotification(int maxGenerationThreshold, int largeObjectHeapThreshold)
        {
            if ((maxGenerationThreshold <= 0) || (maxGenerationThreshold >= 100))
            {
                throw new ArgumentOutOfRangeException("maxGenerationThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), new object[] { 1, 0x63 }));
            }
            if ((largeObjectHeapThreshold <= 0) || (largeObjectHeapThreshold >= 100))
            {
                throw new ArgumentOutOfRangeException("largeObjectHeapThreshold", string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("ArgumentOutOfRange_Bounds_Lower_Upper"), new object[] { 1, 0x63 }));
            }
            if (!_RegisterForFullGCNotification(maxGenerationThreshold, largeObjectHeapThreshold))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotWithConcurrentGC"));
            }
        }

        [SecurityCritical]
        public static void RemoveMemoryPressure(long bytesAllocated)
        {
            if (bytesAllocated <= 0L)
            {
                throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if ((4 == IntPtr.Size) && (bytesAllocated > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("bytesAllocated", Environment.GetResourceString("ArgumentOutOfRange_MustBeNonNegInt32"));
            }
            _RemoveMemoryPressure((ulong) bytesAllocated);
        }

        [SecuritySafeCritical]
        public static void ReRegisterForFinalize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            _ReRegisterForFinalize(obj);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern void SetCleanupCache();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void SetGCLatencyMode(int newLatencyMode);
        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void SuppressFinalize(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            _SuppressFinalize(obj);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCApproach()
        {
            return (GCNotificationStatus) _WaitForFullGCApproach(-1);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCApproach(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return (GCNotificationStatus) _WaitForFullGCApproach(millisecondsTimeout);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCComplete()
        {
            return (GCNotificationStatus) _WaitForFullGCComplete(-1);
        }

        [SecurityCritical]
        public static GCNotificationStatus WaitForFullGCComplete(int millisecondsTimeout)
        {
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            return (GCNotificationStatus) _WaitForFullGCComplete(millisecondsTimeout);
        }

        [SecuritySafeCritical]
        public static void WaitForPendingFinalizers()
        {
            _WaitForPendingFinalizers();
        }

        public static int MaxGeneration
        {
            [SecuritySafeCritical]
            get
            {
                return GetMaxGeneration();
            }
        }
    }
}

