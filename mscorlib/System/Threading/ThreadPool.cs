namespace System.Threading
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public static class ThreadPool
    {
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool AdjustThreadsInPool(uint QueueLength);
        [SecuritySafeCritical, Obsolete("ThreadPool.BindHandle(IntPtr) has been deprecated.  Please use ThreadPool.BindHandle(SafeHandle) instead.", false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static bool BindHandle(IntPtr osHandle)
        {
            return BindIOCompletionCallbackNative(osHandle);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public static bool BindHandle(SafeHandle osHandle)
        {
            if (osHandle == null)
            {
                throw new ArgumentNullException("osHandle");
            }
            bool flag = false;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                osHandle.DangerousAddRef(ref success);
                flag = BindIOCompletionCallbackNative(osHandle.DangerousGetHandle());
            }
            finally
            {
                if (success)
                {
                    osHandle.DangerousRelease();
                }
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        private static extern bool BindIOCompletionCallbackNative(IntPtr fileHandle);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void ClearAppDomainRequestActive();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool CompleteThreadPoolRequest(uint QueueLength);
        [SecurityCritical]
        private static void EnsureVMInitialized()
        {
            if (!ThreadPoolGlobals.vmTpInitialized)
            {
                InitializeVMTp(ref ThreadPoolGlobals.enableWorkerTracking);
                ThreadPoolGlobals.vmTpInitialized = true;
            }
        }

        internal static IEnumerable<IThreadPoolWorkItem> EnumerateQueuedWorkItems(ThreadPoolWorkQueue.WorkStealingQueue[] wsQueues, ThreadPoolWorkQueue.QueueSegment globalQueueTail)
        {
            if (wsQueues != null)
            {
                foreach (ThreadPoolWorkQueue.WorkStealingQueue iteratorVariable0 in wsQueues)
                {
                    if ((iteratorVariable0 != null) && (iteratorVariable0.m_array != null))
                    {
                        foreach (IThreadPoolWorkItem iteratorVariable3 in iteratorVariable0.m_array)
                        {
                            if (iteratorVariable3 != null)
                            {
                                yield return iteratorVariable3;
                            }
                        }
                    }
                }
            }
            if (globalQueueTail != null)
            {
                for (ThreadPoolWorkQueue.QueueSegment iteratorVariable4 = globalQueueTail; iteratorVariable4 != null; iteratorVariable4 = iteratorVariable4.Next)
                {
                    foreach (IThreadPoolWorkItem iteratorVariable7 in iteratorVariable4.nodes)
                    {
                        if (iteratorVariable7 != null)
                        {
                            yield return iteratorVariable7;
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public static void GetAvailableThreads(out int workerThreads, out int completionPortThreads)
        {
            GetAvailableThreadsNative(out workerThreads, out completionPortThreads);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetAvailableThreadsNative(out int workerThreads, out int completionPortThreads);
        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetGloballyQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(null, ThreadPoolGlobals.workQueue.queueTail);
        }

        [SecurityCritical]
        internal static object[] GetGloballyQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetGloballyQueuedWorkItems());
        }

        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetLocallyQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(new ThreadPoolWorkQueue.WorkStealingQueue[] { ThreadPoolWorkQueueThreadLocals.threadLocals.workStealingQueue }, null);
        }

        [SecurityCritical]
        internal static object[] GetLocallyQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetLocallyQueuedWorkItems());
        }

        [SecuritySafeCritical]
        public static void GetMaxThreads(out int workerThreads, out int completionPortThreads)
        {
            GetMaxThreadsNative(out workerThreads, out completionPortThreads);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetMaxThreadsNative(out int workerThreads, out int completionPortThreads);
        [SecuritySafeCritical]
        public static void GetMinThreads(out int workerThreads, out int completionPortThreads)
        {
            GetMinThreadsNative(out workerThreads, out completionPortThreads);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetMinThreadsNative(out int workerThreads, out int completionPortThreads);
        [SecurityCritical]
        internal static IEnumerable<IThreadPoolWorkItem> GetQueuedWorkItems()
        {
            return EnumerateQueuedWorkItems(ThreadPoolWorkQueue.allThreadQueues.Current, ThreadPoolGlobals.workQueue.queueTail);
        }

        [SecurityCritical]
        internal static object[] GetQueuedWorkItemsForDebugger()
        {
            return ToObjectArray(GetQueuedWorkItems());
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void InitializeVMTp(ref bool enableWorkerTracking);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool IsThreadPoolHosted();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool NotifyWorkItemComplete();
        [SecuritySafeCritical]
        internal static void NotifyWorkItemProgress()
        {
            if (!ThreadPoolGlobals.vmTpInitialized)
            {
                InitializeVMTp(ref ThreadPoolGlobals.enableWorkerTracking);
            }
            NotifyWorkItemProgressNative();
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void NotifyWorkItemProgressNative();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe bool PostQueuedCompletionStatus(NativeOverlapped* overlapped);
        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static bool QueueUserWorkItem(WaitCallback callBack)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return QueueUserWorkItemHelper(callBack, null, ref lookForMyCaller, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static bool QueueUserWorkItem(WaitCallback callBack, object state)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return QueueUserWorkItemHelper(callBack, state, ref lookForMyCaller, true);
        }

        [SecurityCritical]
        private static bool QueueUserWorkItemHelper(WaitCallback callBack, object state, ref StackCrawlMark stackMark, bool compressStack)
        {
            bool flag = true;
            if (callBack == null)
            {
                throw new ArgumentNullException("WaitCallback");
            }
            EnsureVMInitialized();
            if (ThreadPoolGlobals.useNewWorkerPool)
            {
                try
                {
                    return flag;
                }
                finally
                {
                    QueueUserWorkItemCallback callback = new QueueUserWorkItemCallback(callBack, state, compressStack, ref stackMark);
                    ThreadPoolGlobals.workQueue.Enqueue(callback, true);
                    flag = true;
                }
            }
            _ThreadPoolWaitCallback tpcallBack = new _ThreadPoolWaitCallback(callBack, state, compressStack, ref stackMark);
            int num = ThreadPoolGlobals.tpQueue.EnQueue(tpcallBack);
            if (ThreadPoolGlobals.tpHosted || (num < ThreadPoolGlobals.tpWarmupCount))
            {
                return AdjustThreadsInPool((uint) ThreadPoolGlobals.tpQueue.GetQueueCount());
            }
            UpdateNativeTpCount((uint) ThreadPoolGlobals.tpQueue.GetQueueCount());
            return flag;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            if (millisecondsTimeOutInterval < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            if (millisecondsTimeOutInterval < -1L)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if (totalMilliseconds < -1L)
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (totalMilliseconds > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) totalMilliseconds, executeOnlyOnce, ref lookForMyCaller, true);
        }

        [MethodImpl(MethodImplOptions.NoInlining), CLSCompliant(false), SecuritySafeCritical]
        public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, true);
        }

        [SecurityCritical]
        private static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce, ref StackCrawlMark stackMark, bool compressStack)
        {
            if (RemotingServices.IsTransparentProxy(waitObject))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_WaitOnTransparentProxy"));
            }
            RegisteredWaitHandle registeredWaitHandle = new RegisteredWaitHandle();
            if (callBack == null)
            {
                throw new ArgumentNullException("WaitOrTimerCallback");
            }
            _ThreadPoolWaitOrTimerCallback callback = new _ThreadPoolWaitOrTimerCallback(callBack, state, compressStack, ref stackMark);
            state = callback;
            registeredWaitHandle.SetWaitObject(waitObject);
            IntPtr handle = RegisterWaitForSingleObjectNative(waitObject, state, millisecondsTimeOutInterval, executeOnlyOnce, registeredWaitHandle, ref stackMark, compressStack);
            registeredWaitHandle.SetHandle(handle);
            return registeredWaitHandle;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern IntPtr RegisterWaitForSingleObjectNative(WaitHandle waitHandle, object state, uint timeOutInterval, bool executeOnlyOnce, RegisteredWaitHandle registeredWaitHandle, ref StackCrawlMark stackMark, bool compressStack);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void ReportThreadStatus(bool isWorking);
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool SetAppDomainRequestActive();
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public static bool SetMaxThreads(int workerThreads, int completionPortThreads)
        {
            return SetMaxThreadsNative(workerThreads, completionPortThreads);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool SetMaxThreadsNative(int workerThreads, int completionPortThreads);
        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlThread=true)]
        public static bool SetMinThreads(int workerThreads, int completionPortThreads)
        {
            return SetMinThreadsNative(workerThreads, completionPortThreads);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool SetMinThreadsNative(int workerThreads, int completionPortThreads);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern void SetNativeTpEvent();
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool ShouldReturnToVm();
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool ShouldUseNewWorkerPool();
        private static object[] ToObjectArray(IEnumerable<IThreadPoolWorkItem> workitems)
        {
            int index = 0;
            using (IEnumerator<IThreadPoolWorkItem> enumerator = workitems.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IThreadPoolWorkItem current = enumerator.Current;
                    index++;
                }
            }
            object[] objArray = new object[index];
            index = 0;
            foreach (IThreadPoolWorkItem item in workitems)
            {
                if (index < objArray.Length)
                {
                    objArray[index] = item;
                }
                index++;
            }
            return objArray;
        }

        [SecurityCritical]
        internal static bool TryPopCustomWorkItem(IThreadPoolWorkItem workItem)
        {
            if (!ThreadPoolGlobals.vmTpInitialized)
            {
                return false;
            }
            return ThreadPoolGlobals.workQueue.LocalFindAndPop(workItem);
        }

        [SecurityCritical]
        internal static void UnsafeQueueCustomWorkItem(IThreadPoolWorkItem workItem, bool forceGlobal)
        {
            EnsureVMInitialized();
            try
            {
            }
            finally
            {
                ThreadPoolGlobals.workQueue.Enqueue(workItem, forceGlobal);
            }
        }

        [CLSCompliant(false), SecurityCritical]
        public static unsafe bool UnsafeQueueNativeOverlapped(NativeOverlapped* overlapped)
        {
            return PostQueuedCompletionStatus(overlapped);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static bool UnsafeQueueUserWorkItem(WaitCallback callBack, object state)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return QueueUserWorkItemHelper(callBack, state, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            if (millisecondsTimeOutInterval < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            if (millisecondsTimeOutInterval < -1L)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeOutInterval", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, TimeSpan timeout, bool executeOnlyOnce)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if (totalMilliseconds < -1L)
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegOrNegative1"));
            }
            if (totalMilliseconds > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("ArgumentOutOfRange_LessEqualToIntegerMaxVal"));
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, (uint) totalMilliseconds, executeOnlyOnce, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical, CLSCompliant(false)]
        public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return RegisterWaitForSingleObject(waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce, ref lookForMyCaller, false);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void UpdateNativeTpCount(uint QueueLength);

    }
}

