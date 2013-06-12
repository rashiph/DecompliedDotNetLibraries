namespace System.Web
{
    using System;
    using System.Collections;
    using System.Threading;
    using System.Web.Hosting;

    internal class RequestQueue
    {
        private TimeSpan _clientConnectedTime;
        private int _count;
        private bool _draining;
        private Queue _externQueue;
        private bool _iis6;
        private Queue _localQueue;
        private int _minExternFreeThreads;
        private int _minLocalFreeThreads;
        private int _queueLimit;
        private Timer _timer;
        private readonly TimeSpan _timerPeriod;
        private WaitCallback _workItemCallback;
        private int _workItemCount;
        private const int _workItemLimit = 2;

        internal RequestQueue(int minExternFreeThreads, int minLocalFreeThreads, int queueLimit, TimeSpan clientConnectedTime)
        {
            int num;
            int num2;
            this._localQueue = new Queue();
            this._externQueue = new Queue();
            this._timerPeriod = new TimeSpan(0, 0, 10);
            this._minExternFreeThreads = minExternFreeThreads;
            this._minLocalFreeThreads = minLocalFreeThreads;
            this._queueLimit = queueLimit;
            this._clientConnectedTime = clientConnectedTime;
            this._workItemCallback = new WaitCallback(this.WorkItemCallback);
            this._timer = new Timer(new TimerCallback(this.TimerCompletionCallback), null, this._timerPeriod, this._timerPeriod);
            this._iis6 = HostingEnvironment.IsUnderIIS6Process;
            ThreadPool.GetMaxThreads(out num, out num2);
            UnsafeNativeMethods.SetMinRequestsExecutingToDetectDeadlock(num - minExternFreeThreads);
        }

        private bool CheckClientConnected(HttpWorkerRequest wr)
        {
            if ((DateTime.UtcNow - wr.GetStartTime()) > this._clientConnectedTime)
            {
                return wr.IsClientConnected();
            }
            return true;
        }

        private HttpWorkerRequest DequeueRequest(bool localOnly)
        {
            HttpWorkerRequest workerRequest = null;
            while (this._count > 0)
            {
                lock (this)
                {
                    if (this._localQueue.Count > 0)
                    {
                        workerRequest = (HttpWorkerRequest) this._localQueue.Dequeue();
                        this._count--;
                    }
                    else if (!localOnly && (this._externQueue.Count > 0))
                    {
                        workerRequest = (HttpWorkerRequest) this._externQueue.Dequeue();
                        this._count--;
                    }
                }
                if (workerRequest == null)
                {
                    return workerRequest;
                }
                PerfCounters.DecrementGlobalCounter(GlobalPerfCounter.REQUESTS_QUEUED);
                PerfCounters.DecrementCounter(AppPerfCounter.REQUESTS_IN_APPLICATION_QUEUE);
                if (EtwTrace.IsTraceEnabled(4, 1))
                {
                    EtwTrace.Trace(EtwTraceType.ETW_TYPE_REQ_DEQUEUED, workerRequest);
                }
                if (this.CheckClientConnected(workerRequest))
                {
                    return workerRequest;
                }
                HttpRuntime.RejectRequestNow(workerRequest, true);
                workerRequest = null;
                PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_DISCONNECTED);
                PerfCounters.IncrementCounter(AppPerfCounter.APP_REQUEST_DISCONNECTED);
            }
            return workerRequest;
        }

        internal void Drain()
        {
            this._draining = true;
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
            while (this._workItemCount > 0)
            {
                Thread.Sleep(100);
            }
            if (this._count == 0)
            {
                return;
            }
            while (true)
            {
                HttpWorkerRequest wr = this.DequeueRequest(false);
                if (wr == null)
                {
                    return;
                }
                HttpRuntime.RejectRequestNow(wr, false);
            }
        }

        internal HttpWorkerRequest GetRequestToExecute(HttpWorkerRequest wr)
        {
            int num;
            int num2;
            int num3;
            ThreadPool.GetAvailableThreads(out num, out num2);
            if (this._iis6)
            {
                num3 = num;
            }
            else
            {
                num3 = (num2 > num) ? num : num2;
            }
            if ((num3 < this._minExternFreeThreads) || (this._count != 0))
            {
                bool isLocal = IsLocal(wr);
                if ((isLocal && (num3 >= this._minLocalFreeThreads)) && (this._count == 0))
                {
                    return wr;
                }
                if (this._count >= this._queueLimit)
                {
                    HttpRuntime.RejectRequestNow(wr, false);
                    return null;
                }
                this.QueueRequest(wr, isLocal);
                if (num3 >= this._minExternFreeThreads)
                {
                    wr = this.DequeueRequest(false);
                    return wr;
                }
                if (num3 >= this._minLocalFreeThreads)
                {
                    wr = this.DequeueRequest(true);
                    return wr;
                }
                wr = null;
                this.ScheduleMoreWorkIfNeeded();
            }
            return wr;
        }

        private static bool IsLocal(HttpWorkerRequest wr)
        {
            string remoteAddress = wr.GetRemoteAddress();
            switch (remoteAddress)
            {
                case "127.0.0.1":
                case "::1":
                    return true;
            }
            if (string.IsNullOrEmpty(remoteAddress))
            {
                return false;
            }
            return (remoteAddress == wr.GetLocalAddress());
        }

        private void QueueRequest(HttpWorkerRequest wr, bool isLocal)
        {
            lock (this)
            {
                if (isLocal)
                {
                    this._localQueue.Enqueue(wr);
                }
                else
                {
                    this._externQueue.Enqueue(wr);
                }
                this._count++;
            }
            PerfCounters.IncrementGlobalCounter(GlobalPerfCounter.REQUESTS_QUEUED);
            PerfCounters.IncrementCounter(AppPerfCounter.REQUESTS_IN_APPLICATION_QUEUE);
            if (EtwTrace.IsTraceEnabled(4, 1))
            {
                EtwTrace.Trace(EtwTraceType.ETW_TYPE_REQ_QUEUED, wr);
            }
        }

        internal void ScheduleMoreWorkIfNeeded()
        {
            if ((!this._draining && (this._count != 0)) && (this._workItemCount < 2))
            {
                int num;
                int num2;
                ThreadPool.GetAvailableThreads(out num, out num2);
                if (num >= this._minLocalFreeThreads)
                {
                    Interlocked.Increment(ref this._workItemCount);
                    ThreadPool.QueueUserWorkItem(this._workItemCallback);
                }
            }
        }

        private void TimerCompletionCallback(object state)
        {
            this.ScheduleMoreWorkIfNeeded();
        }

        private void WorkItemCallback(object state)
        {
            Interlocked.Decrement(ref this._workItemCount);
            if (!this._draining && (this._count != 0))
            {
                int num;
                int num2;
                ThreadPool.GetAvailableThreads(out num, out num2);
                if (num >= this._minLocalFreeThreads)
                {
                    HttpWorkerRequest wr = this.DequeueRequest(num < this._minExternFreeThreads);
                    if (wr != null)
                    {
                        this.ScheduleMoreWorkIfNeeded();
                        HttpRuntime.ProcessRequestNow(wr);
                    }
                }
            }
        }

        internal bool IsEmpty
        {
            get
            {
                return (this._count == 0);
            }
        }
    }
}

