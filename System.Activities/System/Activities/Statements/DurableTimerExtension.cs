namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Activities.Persistence;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml.Linq;

    public class DurableTimerExtension : TimerExtension, IWorkflowInstanceExtension, IDisposable, ICancelable
    {
        private WorkflowInstanceProxy instance;
        private bool isDisposed;
        private static AsyncCallback onResumeBookmarkComplete = Fx.ThunkCallback(new AsyncCallback(DurableTimerExtension.OnResumeBookmarkComplete));
        private Action<object> onTimerFiredCallback;
        private TimerTable registeredTimers;
        private object thisLock;
        private static readonly XName timerExpirationTimeName = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties").GetName("TimerExpirationTime");
        private TimerPersistenceParticipant timerPersistenceParticipant;
        private static readonly XName timerTableName = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties").GetName("RegisteredTimers");

        public DurableTimerExtension()
        {
            this.onTimerFiredCallback = new Action<object>(this.OnTimerFired);
            this.thisLock = new object();
            this.timerPersistenceParticipant = new TimerPersistenceParticipant(this);
            this.isDisposed = false;
        }

        public void Dispose()
        {
            if (this.registeredTimers != null)
            {
                lock (this.ThisLock)
                {
                    this.isDisposed = true;
                    if (this.registeredTimers != null)
                    {
                        this.registeredTimers.Dispose();
                    }
                }
            }
            GC.SuppressFinalize(this);
        }

        public virtual IEnumerable<object> GetAdditionalExtensions()
        {
            yield return this.timerPersistenceParticipant;
        }

        protected override void OnCancelTimer(Bookmark bookmark)
        {
            lock (this.ThisLock)
            {
                this.RegisteredTimers.RemoveTimer(bookmark);
            }
        }

        internal void OnLoad(IDictionary<XName, object> readWriteValues)
        {
            lock (this.ThisLock)
            {
                object obj2;
                if ((readWriteValues != null) && readWriteValues.TryGetValue(timerTableName, out obj2))
                {
                    this.registeredTimers = obj2 as TimerTable;
                    this.RegisteredTimers.OnLoad(this);
                }
            }
        }

        protected override void OnRegisterTimer(TimeSpan timeout, Bookmark bookmark)
        {
            if (timeout < TimeSpan.MaxValue)
            {
                lock (this.ThisLock)
                {
                    this.RegisteredTimers.AddTimer(timeout, bookmark);
                }
            }
        }

        private static void OnResumeBookmarkComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                BookmarkResumptionState asyncState = (BookmarkResumptionState) result.AsyncState;
                try
                {
                    BookmarkResumptionResult result2 = asyncState.Instance.EndResumeBookmark(result);
                    asyncState.TimerExtension.ProcessBookmarkResumptionResult(asyncState.TimerBookmark, result2);
                }
                catch (TimeoutException)
                {
                    asyncState.TimerExtension.ProcessBookmarkResumptionResult(asyncState.TimerBookmark, BookmarkResumptionResult.NotReady);
                }
            }
        }

        internal void OnSave(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
        {
            readWriteValues = null;
            writeOnlyValues = null;
            lock (this.ThisLock)
            {
                this.RegisteredTimers.MarkAsImmutable();
                if ((this.registeredTimers != null) && (this.registeredTimers.Count > 0))
                {
                    readWriteValues = new Dictionary<XName, object>(1);
                    writeOnlyValues = new Dictionary<XName, object>(1);
                    readWriteValues.Add(timerTableName, this.registeredTimers);
                    writeOnlyValues.Add(timerExpirationTimeName, this.registeredTimers.GetNextDueTime());
                }
            }
        }

        private void OnTimerFired(object state)
        {
            Bookmark nextExpiredBookmark;
            lock (this.ThisLock)
            {
                nextExpiredBookmark = this.RegisteredTimers.GetNextExpiredBookmark();
                if ((nextExpiredBookmark == null) || (this.RegisteredTimers.GetNextDueTime() > DateTime.UtcNow))
                {
                    return;
                }
            }
            WorkflowInstanceProxy instance = this.instance;
            if (instance != null)
            {
                IAsyncResult result2 = null;
                bool completedSynchronously = false;
                try
                {
                    result2 = instance.BeginResumeBookmark(nextExpiredBookmark, null, TimeSpan.FromSeconds(2.0), onResumeBookmarkComplete, new BookmarkResumptionState(nextExpiredBookmark, this, instance));
                    completedSynchronously = result2.CompletedSynchronously;
                }
                catch (TimeoutException)
                {
                    this.ProcessBookmarkResumptionResult(nextExpiredBookmark, BookmarkResumptionResult.NotReady);
                }
                if (completedSynchronously && (result2 != null))
                {
                    try
                    {
                        BookmarkResumptionResult result = instance.EndResumeBookmark(result2);
                        this.ProcessBookmarkResumptionResult(nextExpiredBookmark, result);
                    }
                    catch (TimeoutException)
                    {
                        this.ProcessBookmarkResumptionResult(nextExpiredBookmark, BookmarkResumptionResult.NotReady);
                    }
                }
            }
        }

        internal void PersistenceDone()
        {
            lock (this.ThisLock)
            {
                this.RegisteredTimers.MarkAsMutable();
            }
        }

        private void ProcessBookmarkResumptionResult(Bookmark timerBookmark, BookmarkResumptionResult result)
        {
            switch (result)
            {
                case BookmarkResumptionResult.Success:
                case BookmarkResumptionResult.NotFound:
                    lock (this.ThisLock)
                    {
                        if (!this.isDisposed)
                        {
                            this.RegisteredTimers.RemoveTimer(timerBookmark);
                        }
                        return;
                    }
                    break;

                case BookmarkResumptionResult.NotReady:
                    break;

                default:
                    return;
            }
            lock (this.ThisLock)
            {
                this.RegisteredTimers.RetryTimer(timerBookmark);
            }
        }

        public virtual void SetInstance(WorkflowInstanceProxy instance)
        {
            if ((this.instance != null) && (instance != null))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.TimerExtensionAlreadyAttached));
            }
            this.instance = instance;
        }

        void ICancelable.Cancel()
        {
            this.Dispose();
        }

        internal Action<object> OnTimerFiredCallback
        {
            get
            {
                return this.onTimerFiredCallback;
            }
        }

        internal TimerTable RegisteredTimers
        {
            get
            {
                if (this.registeredTimers == null)
                {
                    this.registeredTimers = new TimerTable(this);
                }
                return this.registeredTimers;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }


        private class BookmarkResumptionState
        {
            public BookmarkResumptionState(Bookmark timerBookmark, DurableTimerExtension timerExtension, WorkflowInstanceProxy instance)
            {
                this.TimerBookmark = timerBookmark;
                this.TimerExtension = timerExtension;
                this.Instance = instance;
            }

            public WorkflowInstanceProxy Instance { get; private set; }

            public Bookmark TimerBookmark { get; private set; }

            public DurableTimerExtension TimerExtension { get; private set; }
        }

        private class TimerPersistenceParticipant : PersistenceIOParticipant
        {
            private DurableTimerExtension defaultTimerExtension;

            public TimerPersistenceParticipant(DurableTimerExtension timerExtension) : base(false, false)
            {
                this.defaultTimerExtension = timerExtension;
            }

            protected override void Abort()
            {
                this.defaultTimerExtension.PersistenceDone();
            }

            protected override IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.defaultTimerExtension.PersistenceDone();
                return base.BeginOnSave(readWriteValues, writeOnlyValues, timeout, callback, state);
            }

            protected override void CollectValues(out IDictionary<XName, object> readWriteValues, out IDictionary<XName, object> writeOnlyValues)
            {
                this.defaultTimerExtension.OnSave(out readWriteValues, out writeOnlyValues);
            }

            protected override void PublishValues(IDictionary<XName, object> readWriteValues)
            {
                this.defaultTimerExtension.OnLoad(readWriteValues);
            }
        }
    }
}

