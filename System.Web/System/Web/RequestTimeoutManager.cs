namespace System.Web
{
    using System;
    using System.Collections;
    using System.Threading;
    using System.Web.Util;

    internal class RequestTimeoutManager
    {
        private int _currentList;
        private int _inProgressLock;
        private DoubleLinkList[] _lists = new DoubleLinkList[13];
        private int _requestCount = 0;
        private Timer _timer;
        private readonly TimeSpan _timerPeriod = new TimeSpan(0, 0, 15);

        internal RequestTimeoutManager()
        {
            for (int i = 0; i < this._lists.Length; i++)
            {
                this._lists[i] = new DoubleLinkList();
            }
            this._currentList = 0;
            this._inProgressLock = 0;
            this._timer = new Timer(new TimerCallback(this.TimerCompletionCallback), null, this._timerPeriod, this._timerPeriod);
        }

        internal void Add(HttpContext context)
        {
            if (context.TimeoutLink != null)
            {
                ((RequestTimeoutEntry) context.TimeoutLink).IncrementCount();
            }
            else
            {
                RequestTimeoutEntry entry = new RequestTimeoutEntry(context);
                int index = this._currentList++;
                if (index >= this._lists.Length)
                {
                    index = 0;
                    this._currentList = 0;
                }
                entry.AddToList(this._lists[index]);
                Interlocked.Increment(ref this._requestCount);
                context.TimeoutLink = entry;
            }
        }

        private void CancelTimedOutRequests(DateTime now)
        {
            if (Interlocked.CompareExchange(ref this._inProgressLock, 1, 0) == 0)
            {
                ArrayList list = new ArrayList(this._requestCount);
                for (int i = 0; i < this._lists.Length; i++)
                {
                    lock (this._lists[i])
                    {
                        DoubleLinkListEnumerator enumerator = this._lists[i].GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            list.Add(enumerator.GetDoubleLink());
                        }
                        enumerator = null;
                    }
                }
                int count = list.Count;
                for (int j = 0; j < count; j++)
                {
                    ((RequestTimeoutEntry) list[j]).TimeoutIfNeeded(now);
                }
                Interlocked.Exchange(ref this._inProgressLock, 0);
            }
        }

        internal void Remove(HttpContext context)
        {
            RequestTimeoutEntry timeoutLink = (RequestTimeoutEntry) context.TimeoutLink;
            if (timeoutLink != null)
            {
                if (timeoutLink.DecrementCount() != 0)
                {
                    return;
                }
                timeoutLink.RemoveFromList();
                Interlocked.Decrement(ref this._requestCount);
            }
            context.TimeoutLink = null;
        }

        internal void Stop()
        {
            if (this._timer != null)
            {
                this._timer.Dispose();
                this._timer = null;
            }
            while (this._inProgressLock != 0)
            {
                Thread.Sleep(100);
            }
            if (this._requestCount > 0)
            {
                this.CancelTimedOutRequests(DateTime.UtcNow.AddYears(1));
            }
        }

        private void TimerCompletionCallback(object state)
        {
            if (this._requestCount > 0)
            {
                this.CancelTimedOutRequests(DateTime.UtcNow);
            }
        }

        private class RequestTimeoutEntry : DoubleLink
        {
            private HttpContext _context;
            private int _count;
            private DoubleLinkList _list;

            internal RequestTimeoutEntry(HttpContext context)
            {
                this._context = context;
                this._count = 1;
            }

            internal void AddToList(DoubleLinkList list)
            {
                lock (list)
                {
                    list.InsertTail(this);
                    this._list = list;
                }
            }

            internal int DecrementCount()
            {
                return Interlocked.Decrement(ref this._count);
            }

            internal void IncrementCount()
            {
                Interlocked.Increment(ref this._count);
            }

            internal void RemoveFromList()
            {
                if (this._list != null)
                {
                    lock (this._list)
                    {
                        base.Remove();
                        this._list = null;
                    }
                }
            }

            internal void TimeoutIfNeeded(DateTime now)
            {
                Thread thread = this._context.MustTimeout(now);
                if (thread != null)
                {
                    this.RemoveFromList();
                    thread.Abort(new HttpApplication.CancelModuleException(true));
                }
            }
        }
    }
}

