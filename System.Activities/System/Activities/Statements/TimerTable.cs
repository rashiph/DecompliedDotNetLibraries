namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class TimerTable : IDisposable
    {
        private IOThreadTimer activeTimer;
        private bool isImmutable;
        private Bookmark pendingRemoveBookmark;
        private Bookmark pendingRetryBookmark;
        [DataMember]
        private SortedTimerList sortedTimerList = new SortedTimerList();
        private Dictionary<Bookmark, TimerData> timers = new Dictionary<Bookmark, TimerData>();

        public TimerTable(DurableTimerExtension timerExtension)
        {
            this.activeTimer = new IOThreadTimer(timerExtension.OnTimerFiredCallback, null, false, 0);
        }

        public void AddTimer(TimeSpan timeout, Bookmark bookmark)
        {
            DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, timeout);
            TimerData data = new TimerData(bookmark, expirationTime);
            this.timers.Add(bookmark, data);
            if (this.sortedTimerList.Count == 0)
            {
                this.sortedTimerList.Add(data);
                this.activeTimer.Set(timeout);
            }
            else
            {
                TimerData data2 = this.sortedTimerList.First();
                this.sortedTimerList.Add(data);
                if (data2.ExpirationTime > expirationTime)
                {
                    this.activeTimer.Set(timeout);
                }
            }
        }

        public void Dispose()
        {
            this.activeTimer.Cancel();
            this.timers.Clear();
            this.sortedTimerList.Clear();
            this.pendingRemoveBookmark = null;
            this.pendingRetryBookmark = null;
        }

        public DateTime GetNextDueTime()
        {
            DateTime maxValue = DateTime.MaxValue;
            if (this.sortedTimerList.Count > 0)
            {
                maxValue = this.sortedTimerList.First().ExpirationTime;
            }
            return maxValue;
        }

        public Bookmark GetNextExpiredBookmark()
        {
            Bookmark bookmark = null;
            if (this.sortedTimerList.Count > 0)
            {
                bookmark = this.sortedTimerList.First().Bookmark;
            }
            return bookmark;
        }

        public void MarkAsImmutable()
        {
            this.isImmutable = true;
        }

        public void MarkAsMutable()
        {
            if (this.isImmutable)
            {
                this.isImmutable = false;
                if (this.pendingRemoveBookmark != null)
                {
                    this.RemoveTimer(this.pendingRemoveBookmark);
                    this.pendingRemoveBookmark = null;
                }
                if (this.pendingRetryBookmark != null)
                {
                    this.RetryTimer(this.pendingRetryBookmark);
                    this.pendingRetryBookmark = null;
                }
            }
        }

        public void OnLoad(DurableTimerExtension timerExtension)
        {
            this.timers = new Dictionary<Bookmark, TimerData>();
            this.activeTimer = new IOThreadTimer(timerExtension.OnTimerFiredCallback, null, false, 0);
            foreach (TimerData data in this.sortedTimerList.Timers)
            {
                this.timers.Add(data.Bookmark, data);
            }
            if (this.sortedTimerList.Count > 0)
            {
                TimerData data2 = this.sortedTimerList.First();
                if (data2.ExpirationTime <= DateTime.UtcNow)
                {
                    timerExtension.OnTimerFiredCallback(data2.Bookmark);
                }
                else
                {
                    this.activeTimer.Set((TimeSpan) (data2.ExpirationTime - DateTime.UtcNow));
                }
            }
        }

        public void RemoveTimer(Bookmark bookmark)
        {
            if (!this.isImmutable)
            {
                TimerData data;
                if (this.timers.TryGetValue(bookmark, out data))
                {
                    TimerData data2 = this.sortedTimerList.First();
                    this.timers.Remove(bookmark);
                    this.sortedTimerList.Remove(data);
                    if (data2.Equals(data))
                    {
                        this.activeTimer.Cancel();
                        if (this.sortedTimerList.Count > 0)
                        {
                            TimerData data3 = this.sortedTimerList.First();
                            this.activeTimer.Set((TimeSpan) (data3.ExpirationTime - DateTime.UtcNow));
                        }
                    }
                }
            }
            else
            {
                this.pendingRemoveBookmark = bookmark;
            }
        }

        public void RetryTimer(Bookmark bookmark)
        {
            if (!this.isImmutable)
            {
                TimerData data;
                if (this.timers.TryGetValue(bookmark, out data))
                {
                    this.sortedTimerList.First();
                    this.timers.Remove(bookmark);
                    this.sortedTimerList.Remove(data);
                    DateTime expirationTime = TimeoutHelper.Add(DateTime.UtcNow, TimeSpan.FromSeconds(10.0));
                    TimerData data2 = new TimerData(bookmark, expirationTime);
                    this.timers.Add(bookmark, data2);
                    this.sortedTimerList.Add(data2);
                    TimerData data3 = this.sortedTimerList.First();
                    this.activeTimer.Set((TimeSpan) (data3.ExpirationTime - DateTime.UtcNow));
                }
            }
            else
            {
                this.pendingRetryBookmark = bookmark;
            }
        }

        public int Count
        {
            get
            {
                return this.sortedTimerList.Count;
            }
        }

        [DataContract]
        private class SortedTimerList
        {
            [DataMember]
            private List<TimerTable.TimerData> list = new List<TimerTable.TimerData>();
            private System.Activities.Statements.TimerTable.TimerComparer timerComparer;

            public void Add(TimerTable.TimerData timerData)
            {
                int num = this.list.BinarySearch(timerData, this.TimerComparer);
                if (num < 0)
                {
                    this.list.Insert(~num, timerData);
                }
            }

            public void Clear()
            {
                this.list.Clear();
            }

            public TimerTable.TimerData First()
            {
                return this.list.First<TimerTable.TimerData>();
            }

            public void Remove(TimerTable.TimerData timerData)
            {
                int index = this.list.BinarySearch(timerData, this.TimerComparer);
                if (index >= 0)
                {
                    this.list.RemoveAt(index);
                }
            }

            public int Count
            {
                get
                {
                    return this.list.Count;
                }
            }

            private System.Activities.Statements.TimerTable.TimerComparer TimerComparer
            {
                get
                {
                    if (this.timerComparer == null)
                    {
                        this.timerComparer = new System.Activities.Statements.TimerTable.TimerComparer();
                    }
                    return this.timerComparer;
                }
            }

            public List<TimerTable.TimerData> Timers
            {
                get
                {
                    return this.list;
                }
            }
        }

        private class TimerComparer : IComparer<TimerTable.TimerData>
        {
            public int Compare(TimerTable.TimerData x, TimerTable.TimerData y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return 0;
                }
                if (x == null)
                {
                    return -1;
                }
                if (y == null)
                {
                    return 1;
                }
                if (!(x.ExpirationTime == y.ExpirationTime))
                {
                    return x.ExpirationTime.CompareTo(y.ExpirationTime);
                }
                if (x.Bookmark.IsNamed)
                {
                    if (y.Bookmark.IsNamed)
                    {
                        return string.Compare(x.Bookmark.Name, y.Bookmark.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    return 1;
                }
                if (y.Bookmark.IsNamed)
                {
                    return -1;
                }
                return x.Bookmark.Id.CompareTo(y.Bookmark.Id);
            }
        }

        [DataContract]
        private class TimerData : IEquatable<TimerTable.TimerData>
        {
            public TimerData(System.Activities.Bookmark bookmark, DateTime expirationTime)
            {
                this.Bookmark = bookmark;
                this.ExpirationTime = expirationTime;
            }

            public bool Equals(TimerTable.TimerData other)
            {
                if (this.ExpirationTime == other.ExpirationTime)
                {
                    return this.Bookmark.Equals(other.Bookmark);
                }
                return this.ExpirationTime.Equals(other.ExpirationTime);
            }

            [DataMember]
            public System.Activities.Bookmark Bookmark { get; private set; }

            [DataMember]
            public DateTime ExpirationTime { get; private set; }
        }
    }
}

