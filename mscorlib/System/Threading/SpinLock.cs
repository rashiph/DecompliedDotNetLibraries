namespace System.Threading
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), ComVisible(false), DebuggerDisplay("IsHeld = {IsHeld}"), DebuggerTypeProxy(typeof(SpinLock.SystemThreading_SpinLockDebugView)), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public struct SpinLock
    {
        private const int SPINNING_FACTOR = 100;
        private const int SLEEP_ONE_FREQUENCY = 40;
        private const int SLEEP_ZERO_FREQUENCY = 10;
        private const int TIMEOUT_CHECK_FREQUENCY = 10;
        private const int LOCK_ID_DISABLE_MASK = -2147483648;
        private const int LOCK_ANONYMOUS_OWNED = 1;
        private const int WAITERS_MASK = 0x7ffffffe;
        private const int LOCK_UNOWNED = 0;
        private volatile int m_owner;
        private static int MAXIMUM_WAITERS;
        public SpinLock(bool enableThreadOwnerTracking)
        {
            this.m_owner = 0;
            if (!enableThreadOwnerTracking)
            {
                this.m_owner |= -2147483648;
            }
        }

        public void Enter(ref bool lockTaken)
        {
            if (lockTaken)
            {
                lockTaken = false;
                throw new ArgumentException(Environment.GetResourceString("SpinLock_TryReliableEnter_ArgumentException"));
            }
            int owner = this.m_owner;
            int managedThreadId = 0;
            if ((this.m_owner & -2147483648) == 0)
            {
                if (owner == 0)
                {
                    managedThreadId = Thread.CurrentThread.ManagedThreadId;
                }
            }
            else if ((owner & 1) == 0)
            {
                managedThreadId = owner | 1;
            }
            if (managedThreadId != 0)
            {
                Thread.BeginCriticalRegion();
                if (Interlocked.CompareExchange(ref this.m_owner, managedThreadId, owner, ref lockTaken) == owner)
                {
                    return;
                }
                Thread.EndCriticalRegion();
            }
            this.ContinueTryEnter(-1, ref lockTaken);
        }

        public void TryEnter(ref bool lockTaken)
        {
            this.TryEnter(0, ref lockTaken);
        }

        public void TryEnter(TimeSpan timeout, ref bool lockTaken)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout", timeout, Environment.GetResourceString("SpinLock_TryEnter_ArgumentOutOfRange"));
            }
            this.TryEnter((int) timeout.TotalMilliseconds, ref lockTaken);
        }

        public void TryEnter(int millisecondsTimeout, ref bool lockTaken)
        {
            if (lockTaken)
            {
                lockTaken = false;
                throw new ArgumentException(Environment.GetResourceString("SpinLock_TryReliableEnter_ArgumentException"));
            }
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout", millisecondsTimeout, Environment.GetResourceString("SpinLock_TryEnter_ArgumentOutOfRange"));
            }
            int owner = this.m_owner;
            int managedThreadId = 0;
            if (this.IsThreadOwnerTrackingEnabled)
            {
                if (owner == 0)
                {
                    managedThreadId = Thread.CurrentThread.ManagedThreadId;
                }
            }
            else if ((owner & 1) == 0)
            {
                managedThreadId = owner | 1;
            }
            if (managedThreadId != 0)
            {
                Thread.BeginCriticalRegion();
                if (Interlocked.CompareExchange(ref this.m_owner, managedThreadId, owner, ref lockTaken) == owner)
                {
                    return;
                }
                Thread.EndCriticalRegion();
            }
            this.ContinueTryEnter(millisecondsTimeout, ref lockTaken);
        }

        private void ContinueTryEnter(int millisecondsTimeout, ref bool lockTaken)
        {
            int owner;
            long startTicks = 0L;
            if ((millisecondsTimeout != -1) && (millisecondsTimeout != 0))
            {
                startTicks = DateTime.UtcNow.Ticks;
            }
            if (CdsSyncEtwBCLProvider.Log.IsEnabled())
            {
                CdsSyncEtwBCLProvider.Log.SpinLock_FastPathFailed(this.m_owner);
            }
            if (this.IsThreadOwnerTrackingEnabled)
            {
                this.ContinueTryEnterWithThreadTracking(millisecondsTimeout, startTicks, ref lockTaken);
                return;
            }
            SpinWait wait = new SpinWait();
            while (true)
            {
                owner = this.m_owner;
                if ((owner & 1) == 0)
                {
                    Thread.BeginCriticalRegion();
                    if (Interlocked.CompareExchange(ref this.m_owner, owner | 1, owner, ref lockTaken) == owner)
                    {
                        return;
                    }
                    Thread.EndCriticalRegion();
                }
                else if (((owner & 0x7ffffffe) == MAXIMUM_WAITERS) || (Interlocked.CompareExchange(ref this.m_owner, owner + 2, owner) == owner))
                {
                    break;
                }
                wait.SpinOnce();
            }
            if ((millisecondsTimeout == 0) || ((millisecondsTimeout != -1) && TimeoutExpired(startTicks, millisecondsTimeout)))
            {
                this.DecrementWaiters();
                return;
            }
            int num3 = ((owner + 2) & 0x7ffffffe) / 2;
            int processorCount = PlatformHelper.ProcessorCount;
            if (num3 < processorCount)
            {
                int num5 = 1;
                for (int i = 1; i <= (num3 * 100); i++)
                {
                    Thread.SpinWait(((num3 + i) * 100) * num5);
                    if (num5 < processorCount)
                    {
                        num5++;
                    }
                    owner = this.m_owner;
                    if ((owner & 1) == 0)
                    {
                        Thread.BeginCriticalRegion();
                        int num7 = ((owner & 0x7ffffffe) == 0) ? (owner | 1) : ((owner - 2) | 1);
                        if (Interlocked.CompareExchange(ref this.m_owner, num7, owner, ref lockTaken) == owner)
                        {
                            return;
                        }
                        Thread.EndCriticalRegion();
                    }
                }
            }
            if ((millisecondsTimeout != -1) && TimeoutExpired(startTicks, millisecondsTimeout))
            {
                this.DecrementWaiters();
                return;
            }
            int num8 = 0;
        Label_015F:
            owner = this.m_owner;
            if ((owner & 1) == 0)
            {
                Thread.BeginCriticalRegion();
                int num9 = ((owner & 0x7ffffffe) == 0) ? (owner | 1) : ((owner - 2) | 1);
                if (Interlocked.CompareExchange(ref this.m_owner, num9, owner, ref lockTaken) == owner)
                {
                    return;
                }
                Thread.EndCriticalRegion();
            }
            if ((num8 % 40) == 0)
            {
                Thread.Sleep(1);
            }
            else if ((num8 % 10) == 0)
            {
                Thread.Sleep(0);
            }
            else
            {
                Thread.Yield();
            }
            if ((((num8 % 10) == 0) && (millisecondsTimeout != -1)) && TimeoutExpired(startTicks, millisecondsTimeout))
            {
                this.DecrementWaiters();
            }
            else
            {
                num8++;
                goto Label_015F;
            }
        }

        private void DecrementWaiters()
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                int owner = this.m_owner;
                if (((owner & 0x7ffffffe) == 0) || (Interlocked.CompareExchange(ref this.m_owner, owner - 2, owner) == owner))
                {
                    return;
                }
                wait.SpinOnce();
            }
        }

        private void ContinueTryEnterWithThreadTracking(int millisecondsTimeout, long startTicks, ref bool lockTaken)
        {
            int comparand = 0;
            int managedThreadId = Thread.CurrentThread.ManagedThreadId;
            if (this.m_owner == managedThreadId)
            {
                throw new LockRecursionException(Environment.GetResourceString("SpinLock_TryEnter_LockRecursionException"));
            }
            SpinWait wait = new SpinWait();
            do
            {
                wait.SpinOnce();
                if (this.m_owner == comparand)
                {
                    Thread.BeginCriticalRegion();
                    if (Interlocked.CompareExchange(ref this.m_owner, managedThreadId, comparand, ref lockTaken) == comparand)
                    {
                        return;
                    }
                    Thread.EndCriticalRegion();
                }
            }
            while ((millisecondsTimeout != 0) && (((millisecondsTimeout == -1) || !wait.NextSpinWillYield) || !TimeoutExpired(startTicks, millisecondsTimeout)));
        }

        private static bool TimeoutExpired(long startTicks, int originalWaitTime)
        {
            long num = DateTime.UtcNow.Ticks - startTicks;
            return (num >= (originalWaitTime * 0x2710L));
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Exit()
        {
            this.Exit(true);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void Exit(bool useMemoryBarrier)
        {
            if (this.IsThreadOwnerTrackingEnabled && !this.IsHeldByCurrentThread)
            {
                throw new SynchronizationLockException(Environment.GetResourceString("SpinLock_Exit_SynchronizationLockException"));
            }
            if (useMemoryBarrier)
            {
                if (this.IsThreadOwnerTrackingEnabled)
                {
                    Interlocked.Exchange(ref this.m_owner, 0);
                }
                else
                {
                    Interlocked.Decrement(ref this.m_owner);
                }
            }
            else if (this.IsThreadOwnerTrackingEnabled)
            {
                this.m_owner = 0;
            }
            else
            {
                int owner = this.m_owner;
                this.m_owner = owner - 1;
            }
            Thread.EndCriticalRegion();
        }

        public bool IsHeld
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                if (this.IsThreadOwnerTrackingEnabled)
                {
                    return (this.m_owner != 0);
                }
                return ((this.m_owner & 1) != 0);
            }
        }
        public bool IsHeldByCurrentThread
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                if (!this.IsThreadOwnerTrackingEnabled)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("SpinLock_IsHeldByCurrentThread"));
                }
                return ((this.m_owner & 0x7fffffff) == Thread.CurrentThread.ManagedThreadId);
            }
        }
        public bool IsThreadOwnerTrackingEnabled
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                return ((this.m_owner & -2147483648) == 0);
            }
        }
        static SpinLock()
        {
            MAXIMUM_WAITERS = 0x7ffffffe;
        }
        internal class SystemThreading_SpinLockDebugView
        {
            private SpinLock m_spinLock;

            public SystemThreading_SpinLockDebugView(SpinLock spinLock)
            {
                this.m_spinLock = spinLock;
            }

            public bool IsHeld
            {
                get
                {
                    return this.m_spinLock.IsHeld;
                }
            }

            public bool? IsHeldByCurrentThread
            {
                get
                {
                    try
                    {
                        return new bool?(this.m_spinLock.IsHeldByCurrentThread);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                }
            }

            public int? OwnerThreadID
            {
                get
                {
                    if (this.m_spinLock.IsThreadOwnerTrackingEnabled)
                    {
                        return new int?(this.m_spinLock.m_owner);
                    }
                    return null;
                }
            }
        }
    }
}

