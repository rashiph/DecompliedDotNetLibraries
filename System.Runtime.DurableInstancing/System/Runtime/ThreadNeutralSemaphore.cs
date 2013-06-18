namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ThreadNeutralSemaphore
    {
        private bool aborted;
        private Func<Exception> abortedExceptionGenerator;
        private int count;
        private static Action<object, TimeoutException> enteredAsyncCallback;
        private int maxCount;
        private object ThisLock;
        private Queue<AsyncWaitHandle> waiters;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ThreadNeutralSemaphore(int maxCount) : this(maxCount, null)
        {
        }

        public ThreadNeutralSemaphore(int maxCount, Func<Exception> abortedExceptionGenerator)
        {
            this.ThisLock = new object();
            this.maxCount = maxCount;
            this.abortedExceptionGenerator = abortedExceptionGenerator;
        }

        public void Abort()
        {
            lock (this.ThisLock)
            {
                if (!this.aborted)
                {
                    this.aborted = true;
                    if (this.waiters != null)
                    {
                        while (this.waiters.Count > 0)
                        {
                            this.waiters.Dequeue().Set();
                        }
                    }
                }
            }
        }

        internal static TimeoutException CreateEnterTimedOutException(TimeSpan timeout)
        {
            return new TimeoutException(SRCore.LockTimeoutExceptionMessage(timeout));
        }

        private Exception CreateObjectAbortedException()
        {
            if (this.abortedExceptionGenerator != null)
            {
                return this.abortedExceptionGenerator();
            }
            return new OperationCanceledException(SRCore.ThreadNeutralSemaphoreAborted);
        }

        public void Enter(TimeSpan timeout)
        {
            if (!this.TryEnter(timeout))
            {
                throw Fx.Exception.AsError(CreateEnterTimedOutException(timeout));
            }
        }

        public bool EnterAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
        {
            AsyncWaitHandle item = null;
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    throw Fx.Exception.AsError(this.CreateObjectAbortedException());
                }
                if (this.count < this.maxCount)
                {
                    this.count++;
                    return true;
                }
                item = new AsyncWaitHandle();
                this.Waiters.Enqueue(item);
            }
            return item.WaitAsync(EnteredAsyncCallback, new EnterAsyncData(this, item, callback, state), timeout);
        }

        private AsyncWaitHandle EnterCore()
        {
            AsyncWaitHandle handle;
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    throw Fx.Exception.AsError(this.CreateObjectAbortedException());
                }
                if (this.count < this.maxCount)
                {
                    this.count++;
                    return null;
                }
                handle = new AsyncWaitHandle();
                this.Waiters.Enqueue(handle);
            }
            return handle;
        }

        public int Exit()
        {
            AsyncWaitHandle handle;
            int count = -1;
            lock (this.ThisLock)
            {
                if (this.aborted)
                {
                    return count;
                }
                if (this.count == 0)
                {
                    string invalidSemaphoreExit = SRCore.InvalidSemaphoreExit;
                    throw Fx.Exception.AsError(new SynchronizationLockException(invalidSemaphoreExit));
                }
                if ((this.waiters == null) || (this.waiters.Count == 0))
                {
                    this.count--;
                    return this.count;
                }
                handle = this.waiters.Dequeue();
                count = this.count;
            }
            handle.Set();
            return count;
        }

        private static void OnEnteredAsync(object state, TimeoutException exception)
        {
            EnterAsyncData data = (EnterAsyncData) state;
            ThreadNeutralSemaphore semaphore = data.Semaphore;
            Exception asyncException = exception;
            if ((exception != null) && !semaphore.RemoveWaiter(data.Waiter))
            {
                asyncException = null;
            }
            if (semaphore.aborted)
            {
                asyncException = semaphore.CreateObjectAbortedException();
            }
            data.Callback(data.State, asyncException);
        }

        private bool RemoveWaiter(AsyncWaitHandle waiter)
        {
            bool flag = false;
            lock (this.ThisLock)
            {
                for (int i = this.Waiters.Count; i > 0; i--)
                {
                    AsyncWaitHandle objA = this.Waiters.Dequeue();
                    if (object.ReferenceEquals(objA, waiter))
                    {
                        flag = true;
                    }
                    else
                    {
                        this.Waiters.Enqueue(objA);
                    }
                }
            }
            return flag;
        }

        public bool TryEnter()
        {
            lock (this.ThisLock)
            {
                if (this.count < this.maxCount)
                {
                    this.count++;
                    return true;
                }
                return false;
            }
        }

        public bool TryEnter(TimeSpan timeout)
        {
            AsyncWaitHandle waiter = this.EnterCore();
            if (waiter == null)
            {
                return true;
            }
            bool flag = !waiter.Wait(timeout);
            if (this.aborted)
            {
                throw Fx.Exception.AsError(this.CreateObjectAbortedException());
            }
            if (flag && !this.RemoveWaiter(waiter))
            {
                flag = false;
            }
            return !flag;
        }

        private static Action<object, TimeoutException> EnteredAsyncCallback
        {
            get
            {
                if (enteredAsyncCallback == null)
                {
                    enteredAsyncCallback = new Action<object, TimeoutException>(ThreadNeutralSemaphore.OnEnteredAsync);
                }
                return enteredAsyncCallback;
            }
        }

        private Queue<AsyncWaitHandle> Waiters
        {
            get
            {
                if (this.waiters == null)
                {
                    this.waiters = new Queue<AsyncWaitHandle>();
                }
                return this.waiters;
            }
        }

        private class EnterAsyncData
        {
            public EnterAsyncData(ThreadNeutralSemaphore semaphore, AsyncWaitHandle waiter, FastAsyncCallback callback, object state)
            {
                this.Waiter = waiter;
                this.Semaphore = semaphore;
                this.Callback = callback;
                this.State = state;
            }

            public FastAsyncCallback Callback { get; set; }

            public ThreadNeutralSemaphore Semaphore { get; set; }

            public object State { get; set; }

            public AsyncWaitHandle Waiter { get; set; }
        }
    }
}

