namespace System.Threading
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    internal sealed class TimerBase : CriticalFinalizerObject, IDisposable
    {
        private IntPtr delegateInfo;
        private int m_lock;
        private int timerDeleted;
        private IntPtr timerHandle;

        [SecurityCritical]
        internal void AddTimer(TimerCallback callback, object state, uint dueTime, uint period, ref StackCrawlMark stackMark)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("TimerCallback");
            }
            _TimerCallback callback2 = new _TimerCallback(callback, state, ref stackMark);
            state = callback2;
            this.AddTimerNative(state, dueTime, period, ref stackMark);
            this.timerDeleted = 0;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern void AddTimerNative(object state, uint dueTime, uint period, ref StackCrawlMark stackMark);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal bool ChangeTimer(uint dueTime, uint period)
        {
            bool flag = false;
            bool flag2 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                do
                {
                    if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                    {
                        flag2 = true;
                        try
                        {
                            if (this.timerDeleted != 0)
                            {
                                throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_Generic"));
                            }
                            flag = this.ChangeTimerNative(dueTime, period);
                        }
                        finally
                        {
                            this.m_lock = 0;
                        }
                    }
                    Thread.SpinWait(1);
                }
                while (!flag2);
            }
            return flag;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool ChangeTimerNative(uint dueTime, uint period);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private extern bool DeleteTimerNative(SafeHandle notifyObject);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecuritySafeCritical]
        public void Dispose()
        {
            bool flag = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                do
                {
                    if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                    {
                        flag = true;
                        try
                        {
                            this.DeleteTimerNative(null);
                        }
                        finally
                        {
                            this.m_lock = 0;
                        }
                    }
                    Thread.SpinWait(1);
                }
                while (!flag);
                GC.SuppressFinalize(this);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), SecurityCritical]
        internal bool Dispose(WaitHandle notifyObject)
        {
            bool flag = false;
            bool flag2 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                do
                {
                    if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                    {
                        flag2 = true;
                        try
                        {
                            flag = this.DeleteTimerNative(notifyObject.SafeWaitHandle);
                        }
                        finally
                        {
                            this.m_lock = 0;
                        }
                    }
                    Thread.SpinWait(1);
                }
                while (!flag2);
                GC.SuppressFinalize(this);
            }
            return flag;
        }

        [SecuritySafeCritical]
        ~TimerBase()
        {
            bool flag = false;
            do
            {
                if (Interlocked.CompareExchange(ref this.m_lock, 1, 0) == 0)
                {
                    flag = true;
                    try
                    {
                        this.DeleteTimerNative(null);
                    }
                    finally
                    {
                        this.m_lock = 0;
                    }
                }
                Thread.SpinWait(1);
            }
            while (!flag);
        }
    }
}

