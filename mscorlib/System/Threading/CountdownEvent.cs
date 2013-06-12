namespace System.Threading
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(false), DebuggerDisplay("Initial Count={InitialCount}, Current Count={CurrentCount}"), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public class CountdownEvent : IDisposable
    {
        private volatile int m_currentCount;
        private volatile bool m_disposed;
        private ManualResetEventSlim m_event;
        private int m_initialCount;

        public CountdownEvent(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }
            this.m_initialCount = initialCount;
            this.m_currentCount = initialCount;
            this.m_event = new ManualResetEventSlim();
            if (initialCount == 0)
            {
                this.m_event.Set();
            }
        }

        public void AddCount()
        {
            this.AddCount(1);
        }

        public void AddCount(int signalCount)
        {
            if (!this.TryAddCount(signalCount))
            {
                throw new InvalidOperationException(Environment.GetResourceString("CountdownEvent_Increment_AlreadyZero"));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_event.Dispose();
                this.m_disposed = true;
            }
        }

        public void Reset()
        {
            this.Reset(this.m_initialCount);
        }

        public void Reset(int count)
        {
            this.ThrowIfDisposed();
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            this.m_currentCount = count;
            this.m_initialCount = count;
            if (count == 0)
            {
                this.m_event.Set();
            }
            else
            {
                this.m_event.Reset();
            }
        }

        public bool Signal()
        {
            return this.Signal(1);
        }

        public bool Signal(int signalCount)
        {
            int num;
            if (signalCount <= 0)
            {
                throw new ArgumentOutOfRangeException("signalCount");
            }
            this.ThrowIfDisposed();
            SpinWait wait = new SpinWait();
        Label_001D:
            num = this.m_currentCount;
            if (num < signalCount)
            {
                throw new InvalidOperationException(Environment.GetResourceString("CountdownEvent_Decrement_BelowZero"));
            }
            if (Interlocked.CompareExchange(ref this.m_currentCount, num - signalCount, num) != num)
            {
                wait.SpinOnce();
                goto Label_001D;
            }
            if (num == signalCount)
            {
                this.m_event.Set();
                return true;
            }
            return false;
        }

        private void ThrowIfDisposed()
        {
            if (this.m_disposed)
            {
                throw new ObjectDisposedException("CountdownEvent");
            }
        }

        public bool TryAddCount()
        {
            return this.TryAddCount(1);
        }

        public bool TryAddCount(int signalCount)
        {
            int num;
            if (signalCount <= 0)
            {
                throw new ArgumentOutOfRangeException("signalCount");
            }
            this.ThrowIfDisposed();
            SpinWait wait = new SpinWait();
        Label_001D:
            num = this.m_currentCount;
            if (num == 0)
            {
                return false;
            }
            if (num > (0x7fffffff - signalCount))
            {
                throw new InvalidOperationException(Environment.GetResourceString("CountdownEvent_Increment_AlreadyMax"));
            }
            if (Interlocked.CompareExchange(ref this.m_currentCount, num + signalCount, num) != num)
            {
                wait.SpinOnce();
                goto Label_001D;
            }
            return true;
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
            if (millisecondsTimeout < -1)
            {
                throw new ArgumentOutOfRangeException("millisecondsTimeout");
            }
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            bool isSet = this.IsSet;
            if (!isSet)
            {
                isSet = this.m_event.Wait(millisecondsTimeout, cancellationToken);
            }
            return isSet;
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

        public int CurrentCount
        {
            get
            {
                return this.m_currentCount;
            }
        }

        public int InitialCount
        {
            get
            {
                return this.m_initialCount;
            }
        }

        public bool IsSet
        {
            get
            {
                return (this.m_currentCount == 0);
            }
        }

        public System.Threading.WaitHandle WaitHandle
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_event.WaitHandle;
            }
        }
    }
}

