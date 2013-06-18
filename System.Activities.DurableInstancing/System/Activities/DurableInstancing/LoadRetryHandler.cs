namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    internal sealed class LoadRetryHandler
    {
        private BinaryHeap<DateTime, LoadRetryAsyncResult> retryQueue = new BinaryHeap<DateTime, LoadRetryAsyncResult>();
        private IOThreadTimer retryThreadTimer;
        private object syncLock = new object();

        public LoadRetryHandler()
        {
            this.retryThreadTimer = new IOThreadTimer(new Action<object>(this.OnRetryTimer), null, false);
        }

        public void AbortPendingRetries()
        {
            ICollection<KeyValuePair<DateTime, LoadRetryAsyncResult>> is2;
            this.retryThreadTimer.Cancel();
            lock (this.syncLock)
            {
                is2 = this.retryQueue.RemoveAll(x => x.Value != null);
            }
            foreach (KeyValuePair<DateTime, LoadRetryAsyncResult> pair in is2)
            {
                ActionItem.Schedule(data => (data as LoadRetryAsyncResult).AbortRetry(), pair.Value);
            }
        }

        public bool Enqueue(LoadRetryAsyncResult command)
        {
            bool flag = false;
            DateTime key = DateTime.UtcNow.Add(command.RetryTimeout);
            lock (this.syncLock)
            {
                flag = this.retryQueue.Enqueue(key, command);
            }
            if (flag)
            {
                this.retryThreadTimer.Set(command.RetryTimeout);
            }
            return true;
        }

        private void OnRetryTimer(object state)
        {
            TimeSpan zero = TimeSpan.Zero;
            bool flag = false;
            lock (this.syncLock)
            {
                if (!this.retryQueue.IsEmpty)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    DateTime key = this.retryQueue.Peek().Key;
                    if (utcNow.CompareTo(key) >= 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        zero = key.Subtract(utcNow);
                    }
                }
            }
            if (flag)
            {
                ICollection<KeyValuePair<DateTime, LoadRetryAsyncResult>> is2;
                object obj3;
                bool lockTaken = false;
                try
                {
                    Monitor.Enter(obj3 = this.syncLock, ref lockTaken);
                    DateTime currentTime = DateTime.UtcNow;
                    is2 = this.retryQueue.TakeWhile(x => currentTime.CompareTo(x) >= 0);
                    if (!this.retryQueue.IsEmpty)
                    {
                        zero = this.retryQueue.Peek().Key.Subtract(currentTime);
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        Monitor.Exit(obj3);
                    }
                }
                foreach (KeyValuePair<DateTime, LoadRetryAsyncResult> pair in is2)
                {
                    pair.Value.Retry();
                }
            }
            if (zero != TimeSpan.Zero)
            {
                this.retryThreadTimer.Set(zero);
            }
        }
    }
}

