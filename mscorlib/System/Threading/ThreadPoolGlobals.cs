namespace System.Threading
{
    using System;
    using System.Security;

    internal static class ThreadPoolGlobals
    {
        public static bool enableWorkerTracking;
        public static int processorCount = Environment.ProcessorCount;
        public static bool tpHosted = ThreadPool.IsThreadPoolHosted();
        public static uint tpQuantum = (ThreadPool.ShouldUseNewWorkerPool() ? 30 : 2);
        public static ThreadPoolRequestQueue tpQueue = (ThreadPool.ShouldUseNewWorkerPool() ? null : new ThreadPoolRequestQueue());
        public static int tpWarmupCount = (Environment.ProcessorCount * 2);
        public static bool useNewWorkerPool = ThreadPool.ShouldUseNewWorkerPool();
        public static bool vmTpInitialized;
        [SecurityCritical]
        public static ThreadPoolWorkQueue workQueue = (ThreadPool.ShouldUseNewWorkerPool() ? new ThreadPoolWorkQueue() : null);
    }
}

