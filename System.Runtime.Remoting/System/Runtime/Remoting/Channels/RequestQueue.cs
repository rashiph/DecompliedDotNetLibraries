namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;
    using System.Threading;

    internal class RequestQueue
    {
        private int _count;
        private Queue _externQueue = new Queue();
        private Queue _localQueue = new Queue();
        private int _minExternFreeThreads;
        private int _minLocalFreeThreads;
        private int _queueLimit;
        private WaitCallback _workItemCallback;
        private int _workItemCount;
        private const int _workItemLimit = 2;

        internal RequestQueue(int minExternFreeThreads, int minLocalFreeThreads, int queueLimit)
        {
            this._minExternFreeThreads = minExternFreeThreads;
            this._minLocalFreeThreads = minLocalFreeThreads;
            this._queueLimit = queueLimit;
            this._workItemCallback = new WaitCallback(this.WorkItemCallback);
        }

        private SocketHandler DequeueRequest(bool localOnly)
        {
            object obj2 = null;
            if (this._count > 0)
            {
                lock (this)
                {
                    if (this._localQueue.Count > 0)
                    {
                        obj2 = this._localQueue.Dequeue();
                        this._count--;
                    }
                    else if (!localOnly && (this._externQueue.Count > 0))
                    {
                        obj2 = this._externQueue.Dequeue();
                        this._count--;
                    }
                }
            }
            return (SocketHandler) obj2;
        }

        internal SocketHandler GetRequestToExecute(SocketHandler sh)
        {
            int num;
            int num2;
            ThreadPool.GetAvailableThreads(out num, out num2);
            int num3 = (num2 > num) ? num : num2;
            if ((num3 < this._minExternFreeThreads) || (this._count != 0))
            {
                bool isLocal = IsLocal(sh);
                if ((isLocal && (num3 >= this._minLocalFreeThreads)) && (this._count == 0))
                {
                    return sh;
                }
                if (this._count >= this._queueLimit)
                {
                    sh.RejectRequestNowSinceServerIsBusy();
                    return null;
                }
                this.QueueRequest(sh, isLocal);
                if (num3 >= this._minExternFreeThreads)
                {
                    sh = this.DequeueRequest(false);
                }
                else if (num3 >= this._minLocalFreeThreads)
                {
                    sh = this.DequeueRequest(true);
                }
                else
                {
                    sh = null;
                }
                if (sh == null)
                {
                    this.ScheduleMoreWorkIfNeeded();
                }
            }
            return sh;
        }

        private static bool IsLocal(SocketHandler sh)
        {
            return sh.IsLocal();
        }

        internal void ProcessNextRequest(SocketHandler sh)
        {
            sh = this.GetRequestToExecute(sh);
            if (sh != null)
            {
                sh.ProcessRequestNow();
            }
        }

        private void QueueRequest(SocketHandler sh, bool isLocal)
        {
            lock (this)
            {
                if (isLocal)
                {
                    this._localQueue.Enqueue(sh);
                }
                else
                {
                    this._externQueue.Enqueue(sh);
                }
                this._count++;
            }
        }

        internal void ScheduleMoreWorkIfNeeded()
        {
            if ((this._count != 0) && (this._workItemCount < 2))
            {
                Interlocked.Increment(ref this._workItemCount);
                ThreadPool.UnsafeQueueUserWorkItem(this._workItemCallback, null);
            }
        }

        private void WorkItemCallback(object state)
        {
            Interlocked.Decrement(ref this._workItemCount);
            if (this._count != 0)
            {
                int num;
                int num2;
                ThreadPool.GetAvailableThreads(out num, out num2);
                bool flag = false;
                if (num >= this._minLocalFreeThreads)
                {
                    SocketHandler handler = this.DequeueRequest(num < this._minExternFreeThreads);
                    if (handler != null)
                    {
                        handler.ProcessRequestNow();
                        flag = true;
                    }
                }
                if (!flag)
                {
                    Thread.Sleep(250);
                    this.ScheduleMoreWorkIfNeeded();
                }
            }
        }
    }
}

