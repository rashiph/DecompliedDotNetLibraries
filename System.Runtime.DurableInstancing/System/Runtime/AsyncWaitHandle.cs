namespace System.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal class AsyncWaitHandle
    {
        private List<AsyncWaiter> asyncWaiters;
        private bool isSignaled;
        private EventResetMode resetMode;
        private object syncObject;
        private int syncWaiterCount;
        private static Action<object> timerCompleteCallback;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AsyncWaitHandle() : this(EventResetMode.AutoReset)
        {
        }

        public AsyncWaitHandle(EventResetMode resetMode)
        {
            this.resetMode = resetMode;
            this.syncObject = new object();
        }

        private static void OnTimerComplete(object state)
        {
            AsyncWaiter item = (AsyncWaiter) state;
            AsyncWaitHandle parent = item.Parent;
            bool flag = false;
            lock (parent.syncObject)
            {
                if ((parent.asyncWaiters != null) && parent.asyncWaiters.Remove(item))
                {
                    item.TimedOut = true;
                    flag = true;
                }
            }
            item.CancelTimer();
            if (flag)
            {
                item.Call();
            }
        }

        public void Reset()
        {
            this.isSignaled = false;
        }

        public void Set()
        {
            List<AsyncWaiter> asyncWaiters = null;
            AsyncWaiter waiter = null;
            if (!this.isSignaled)
            {
                lock (this.syncObject)
                {
                    if (!this.isSignaled)
                    {
                        if (this.resetMode == EventResetMode.ManualReset)
                        {
                            this.isSignaled = true;
                            Monitor.PulseAll(this.syncObject);
                            asyncWaiters = this.asyncWaiters;
                            this.asyncWaiters = null;
                        }
                        else if (this.syncWaiterCount > 0)
                        {
                            Monitor.Pulse(this.syncObject);
                        }
                        else if ((this.asyncWaiters != null) && (this.asyncWaiters.Count > 0))
                        {
                            waiter = this.asyncWaiters[0];
                            this.asyncWaiters.RemoveAt(0);
                        }
                        else
                        {
                            this.isSignaled = true;
                        }
                    }
                }
            }
            if (asyncWaiters != null)
            {
                foreach (AsyncWaiter waiter2 in asyncWaiters)
                {
                    waiter2.CancelTimer();
                    waiter2.Call();
                }
            }
            if (waiter != null)
            {
                waiter.CancelTimer();
                waiter.Call();
            }
        }

        public bool Wait(TimeSpan timeout)
        {
            if (!this.isSignaled || (this.isSignaled && (this.resetMode == EventResetMode.AutoReset)))
            {
                lock (this.syncObject)
                {
                    if (this.isSignaled && (this.resetMode == EventResetMode.AutoReset))
                    {
                        this.isSignaled = false;
                    }
                    else if (!this.isSignaled)
                    {
                        bool flag = false;
                        try
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.syncWaiterCount++;
                                flag = true;
                            }
                            if (timeout == TimeSpan.MaxValue)
                            {
                                if (!Monitor.Wait(this.syncObject, -1))
                                {
                                    return false;
                                }
                            }
                            else if (!Monitor.Wait(this.syncObject, timeout))
                            {
                                return false;
                            }
                        }
                        finally
                        {
                            if (flag)
                            {
                                this.syncWaiterCount--;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool WaitAsync(Action<object, TimeoutException> callback, object state, TimeSpan timeout)
        {
            if (!this.isSignaled || (this.isSignaled && (this.resetMode == EventResetMode.AutoReset)))
            {
                lock (this.syncObject)
                {
                    if (this.isSignaled && (this.resetMode == EventResetMode.AutoReset))
                    {
                        this.isSignaled = false;
                    }
                    else if (!this.isSignaled)
                    {
                        AsyncWaiter item = new AsyncWaiter(this, callback, state);
                        if (this.asyncWaiters == null)
                        {
                            this.asyncWaiters = new List<AsyncWaiter>();
                        }
                        this.asyncWaiters.Add(item);
                        if (timeout != TimeSpan.MaxValue)
                        {
                            if (timerCompleteCallback == null)
                            {
                                timerCompleteCallback = new Action<object>(AsyncWaitHandle.OnTimerComplete);
                            }
                            item.SetTimer(timerCompleteCallback, item, timeout);
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private class AsyncWaiter : ActionItem
        {
            [SecurityCritical]
            private Action<object, TimeoutException> callback;
            private TimeSpan originalTimeout;
            [SecurityCritical]
            private object state;
            private IOThreadTimer timer;

            [SecuritySafeCritical]
            public AsyncWaiter(AsyncWaitHandle parent, Action<object, TimeoutException> callback, object state)
            {
                this.Parent = parent;
                this.callback = callback;
                this.state = state;
            }

            [SecuritySafeCritical]
            public void Call()
            {
                base.Schedule();
            }

            public void CancelTimer()
            {
                if (this.timer != null)
                {
                    this.timer.Cancel();
                    this.timer = null;
                }
            }

            [SecurityCritical]
            protected override void Invoke()
            {
                this.callback(this.state, this.TimedOut ? new TimeoutException(SRCore.TimeoutOnOperation(this.originalTimeout)) : null);
            }

            public void SetTimer(Action<object> callback, object state, TimeSpan timeout)
            {
                if (this.timer != null)
                {
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.MustCancelOldTimer));
                }
                this.originalTimeout = timeout;
                this.timer = new IOThreadTimer(callback, state, false);
                this.timer.Set(timeout);
            }

            public AsyncWaitHandle Parent
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<Parent>k__BackingField;
                }
                [CompilerGenerated]
                private set
                {
                    this.<Parent>k__BackingField = value;
                }
            }

            public bool TimedOut
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<TimedOut>k__BackingField;
                }
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                set
                {
                    this.<TimedOut>k__BackingField = value;
                }
            }
        }
    }
}

