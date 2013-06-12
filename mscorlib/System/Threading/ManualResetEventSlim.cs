namespace System.Threading
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(false), DebuggerDisplay("Set = {IsSet}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class ManualResetEventSlim : IDisposable
    {
        private const int DEFAULT_SPIN_MP = 10;
        private const int DEFAULT_SPIN_SP = 1;
        private const int Dispose_BitMask = 0x40000000;
        private volatile int m_combinedState;
        private ManualResetEvent m_eventObj;
        private object m_lock;
        private const int NumWaitersState_BitMask = 0x7ffff;
        private const int NumWaitersState_MaxValue = 0x7ffff;
        private const int NumWaitersState_ShiftCount = 0;
        private static Action<object> s_cancellationTokenCallback = new Action<object>(ManualResetEventSlim.CancellationTokenCallback);
        private const int SignalledState_BitMask = -2147483648;
        private const int SignalledState_ShiftCount = 0x1f;
        private const int SpinCountState_BitMask = 0x3ff80000;
        private const int SpinCountState_MaxValue = 0x7ff;
        private const int SpinCountState_ShiftCount = 0x13;

        public ManualResetEventSlim() : this(false)
        {
        }

        public ManualResetEventSlim(bool initialState)
        {
            this.Initialize(initialState, 10);
        }

        public ManualResetEventSlim(bool initialState, int spinCount)
        {
            if (spinCount < 0)
            {
                throw new ArgumentOutOfRangeException("spinCount");
            }
            if (spinCount > 0x7ff)
            {
                throw new ArgumentOutOfRangeException("spinCount", string.Format(Environment.GetResourceString("ManualResetEventSlim_ctor_SpinCountOutOfRange"), 0x7ff));
            }
            this.Initialize(initialState, spinCount);
        }

        private static void CancellationTokenCallback(object obj)
        {
            ManualResetEventSlim slim = obj as ManualResetEventSlim;
            lock (slim.m_lock)
            {
                Monitor.PulseAll(slim.m_lock);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if ((this.m_combinedState & 0x40000000) == 0)
            {
                this.m_combinedState |= 0x40000000;
                if (disposing)
                {
                    ManualResetEvent eventObj = this.m_eventObj;
                    if (eventObj != null)
                    {
                        lock (eventObj)
                        {
                            eventObj.Close();
                            this.m_eventObj = null;
                        }
                    }
                }
            }
        }

        private void EnsureLockObjectCreated()
        {
            if (this.m_lock == null)
            {
                object obj2 = new object();
                Interlocked.CompareExchange(ref this.m_lock, obj2, null);
            }
        }

        private static int ExtractStatePortion(int state, int mask)
        {
            return (state & mask);
        }

        private static int ExtractStatePortionAndShiftRight(int state, int mask, int rightBitShiftCount)
        {
            return ((state & mask) >> rightBitShiftCount);
        }

        private void Initialize(bool initialState, int spinCount)
        {
            this.IsSet = initialState;
            this.SpinCount = PlatformHelper.IsSingleProcessor ? 1 : spinCount;
        }

        private bool LazyInitializeEvent()
        {
            bool isSet = this.IsSet;
            ManualResetEvent event2 = new ManualResetEvent(isSet);
            if (Interlocked.CompareExchange<ManualResetEvent>(ref this.m_eventObj, event2, null) != null)
            {
                event2.Close();
                return false;
            }
            if (this.IsSet != isSet)
            {
                lock (event2)
                {
                    if (this.m_eventObj == event2)
                    {
                        event2.Set();
                    }
                }
            }
            return true;
        }

        public void Reset()
        {
            this.ThrowIfDisposed();
            if (this.m_eventObj != null)
            {
                this.m_eventObj.Reset();
            }
            this.IsSet = false;
        }

        public void Set()
        {
            this.Set(false);
        }

        private void Set(bool duringCancellation)
        {
            this.IsSet = true;
            if (this.Waiters > 0)
            {
                lock (this.m_lock)
                {
                    Monitor.PulseAll(this.m_lock);
                }
            }
            ManualResetEvent eventObj = this.m_eventObj;
            if ((eventObj != null) && !duringCancellation)
            {
                lock (eventObj)
                {
                    if (this.m_eventObj != null)
                    {
                        this.m_eventObj.Set();
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
            if ((this.m_combinedState & 0x40000000) != 0)
            {
                throw new ObjectDisposedException(Environment.GetResourceString("ManualResetEventSlim_Disposed"));
            }
        }

        private void UpdateStateAtomically(int newBits, int updateBitsMask)
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                int combinedState = this.m_combinedState;
                int num2 = (combinedState & ~updateBitsMask) | newBits;
                if (Interlocked.CompareExchange(ref this.m_combinedState, num2, combinedState) == combinedState)
                {
                    return;
                }
                wait.SpinOnce();
            }
        }

        private static int UpdateTimeOut(long startTimeTicks, int originalWaitMillisecondsTimeout)
        {
            long num = (DateTime.UtcNow.Ticks - startTimeTicks) / 0x2710L;
            if (num > 0x7fffffffL)
            {
                return -2;
            }
            int num2 = originalWaitMillisecondsTimeout - ((int) num);
            if (num2 < 0)
            {
                return -1;
            }
            return num2;
        }

        public void Wait()
        {
            this.Wait(-1, new CancellationToken());
        }

        public bool Wait(int millisecondsTimeout)
        {
            return this.Wait(millisecondsTimeout, new CancellationToken());
        }

        public void Wait(CancellationToken cancellationToken)
        {
            this.Wait(-1, cancellationToken);
        }

        public bool Wait(TimeSpan timeout)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.Wait((int) totalMilliseconds, new CancellationToken());
        }

        public bool Wait(int millisecondsTimeout, CancellationToken cancellationToken)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            if (!this.IsSet)
            {
                if (millisecondsTimeout == 0)
                {
                    return false;
                }
                long startTimeTicks = 0L;
                bool flag = false;
                int num2 = millisecondsTimeout;
                if (millisecondsTimeout != -1)
                {
                    startTimeTicks = DateTime.UtcNow.Ticks;
                    flag = true;
                }
                int num3 = 10;
                int num4 = 5;
                int num5 = 20;
                for (int i = 0; i < this.SpinCount; i++)
                {
                    if (this.IsSet)
                    {
                        return true;
                    }
                    if (i < num3)
                    {
                        if (i == (num3 / 2))
                        {
                            Thread.Yield();
                        }
                        else
                        {
                            Thread.SpinWait(Environment.ProcessorCount * (((int) 4) << i));
                        }
                    }
                    else if ((i % num5) == 0)
                    {
                        Thread.Sleep(1);
                    }
                    else if ((i % num4) == 0)
                    {
                        Thread.Sleep(0);
                    }
                    else
                    {
                        Thread.Yield();
                    }
                    if ((i >= 100) && ((i % 10) == 0))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                this.EnsureLockObjectCreated();
                using (cancellationToken.Register(s_cancellationTokenCallback, this))
                {
                    lock (this.m_lock)
                    {
                        while (!this.IsSet)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (flag)
                            {
                                num2 = UpdateTimeOut(startTimeTicks, millisecondsTimeout);
                                if (num2 <= 0)
                                {
                                    return false;
                                }
                            }
                            this.Waiters++;
                            if (this.IsSet)
                            {
                                this.Waiters--;
                                return true;
                            }
                            try
                            {
                                if (!Monitor.Wait(this.m_lock, num2))
                                {
                                    return false;
                                }
                                continue;
                            }
                            finally
                            {
                                this.Waiters--;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            long totalMilliseconds = (long) timeout.TotalMilliseconds;
            if ((totalMilliseconds < -1L) || (totalMilliseconds > 0x7fffffffL))
            {
                throw new ArgumentOutOfRangeException("timeout");
            }
            return this.Wait((int) totalMilliseconds, cancellationToken);
        }

        public bool IsSet
        {
            get
            {
                return (0 != ExtractStatePortion(this.m_combinedState, -2147483648));
            }
            private set
            {
                this.UpdateStateAtomically(((value != null) ? 1 : 0) << 0x1f, -2147483648);
            }
        }

        public int SpinCount
        {
            get
            {
                return ExtractStatePortionAndShiftRight(this.m_combinedState, 0x3ff80000, 0x13);
            }
            private set
            {
                this.m_combinedState = (this.m_combinedState & -1073217537) | (value << 0x13);
            }
        }

        private int Waiters
        {
            get
            {
                return ExtractStatePortionAndShiftRight(this.m_combinedState, 0x7ffff, 0);
            }
            set
            {
                if (value >= 0x7ffff)
                {
                    throw new InvalidOperationException(string.Format(Environment.GetResourceString("ManualResetEventSlim_ctor_TooManyWaiters"), 0x7ffff));
                }
                this.UpdateStateAtomically(value, 0x7ffff);
            }
        }

        public System.Threading.WaitHandle WaitHandle
        {
            get
            {
                this.ThrowIfDisposed();
                if (this.m_eventObj == null)
                {
                    this.LazyInitializeEvent();
                }
                return this.m_eventObj;
            }
        }
    }
}

