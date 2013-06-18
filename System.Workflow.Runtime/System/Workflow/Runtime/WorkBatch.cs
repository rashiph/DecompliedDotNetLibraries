namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Transactions;

    internal sealed class WorkBatch : IWorkBatch, IDisposable
    {
        private WorkBatchCollection _collection;
        private PendingWorkCollection _pendingWorkCollection;
        private WorkBatchState _state;
        private object mutex;

        private WorkBatch()
        {
            this.mutex = new object();
        }

        internal WorkBatch(WorkBatchCollection workBackCollection)
        {
            this.mutex = new object();
            this._pendingWorkCollection = new PendingWorkCollection();
            this._state = WorkBatchState.Usable;
            this._collection = workBackCollection;
        }

        internal void Commit(Transaction transaction)
        {
            lock (this.mutex)
            {
                this._pendingWorkCollection.Commit(transaction);
            }
        }

        internal void Complete(bool succeeded)
        {
            lock (this.mutex)
            {
                if (this._pendingWorkCollection.IsUsable)
                {
                    this._pendingWorkCollection.Complete(succeeded);
                    this._pendingWorkCollection.Dispose();
                    this._state = WorkBatchState.Completed;
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._pendingWorkCollection.Dispose();
                this._pendingWorkCollection = null;
            }
        }

        internal void Merge(WorkBatch batch)
        {
            if (batch != null)
            {
                if (this._pendingWorkCollection == null)
                {
                    throw new ObjectDisposedException("WorkBatch");
                }
                lock (this.mutex)
                {
                    lock (batch.mutex)
                    {
                        foreach (KeyValuePair<IPendingWork, SortedList<long, object>> pair in batch._pendingWorkCollection.WorkItems)
                        {
                            foreach (KeyValuePair<long, object> pair2 in pair.Value)
                            {
                                this._pendingWorkCollection.Add(pair.Key, pair2.Key, pair2.Value);
                            }
                        }
                    }
                    this._state = WorkBatchState.Merged;
                }
            }
        }

        internal void SetWorkBatchCollection(WorkBatchCollection workBatchCollection)
        {
            this._collection = workBatchCollection;
        }

        void IWorkBatch.Add(IPendingWork work, object workItem)
        {
            if (this._pendingWorkCollection == null)
            {
                throw new ObjectDisposedException("WorkBatch");
            }
            lock (this.mutex)
            {
                this._pendingWorkCollection.Add(work, this._collection.GetNextWorkItemOrderId(work), workItem);
            }
        }

        internal int Count
        {
            get
            {
                return this._pendingWorkCollection.WorkItems.Count;
            }
        }

        internal bool IsDirty
        {
            get
            {
                return this._pendingWorkCollection.IsDirty;
            }
        }

        internal sealed class PendingWorkCollection : IDisposable
        {
            private Dictionary<IPendingWork, SortedList<long, object>> Items = new Dictionary<IPendingWork, SortedList<long, object>>();

            internal PendingWorkCollection()
            {
            }

            internal void Add(IPendingWork work, long orderId, object workItem)
            {
                SortedList<long, object> list = null;
                if (!this.Items.TryGetValue(work, out list))
                {
                    list = new SortedList<long, object>();
                    this.Items.Add(work, list);
                }
                list.Add(orderId, workItem);
                WorkflowTrace.Runtime.TraceEvent(TraceEventType.Information, 0, "pending work hc {0} added workItem hc {1}", new object[] { work.GetHashCode(), workItem.GetHashCode() });
            }

            internal void Commit(Transaction transaction)
            {
                foreach (KeyValuePair<IPendingWork, SortedList<long, object>> pair in this.Items)
                {
                    IPendingWork key = pair.Key;
                    List<object> items = new List<object>(pair.Value.Values);
                    key.Commit(transaction, items);
                }
            }

            internal void Complete(bool succeeded)
            {
                foreach (KeyValuePair<IPendingWork, SortedList<long, object>> pair in this.Items)
                {
                    IPendingWork key = pair.Key;
                    List<object> items = new List<object>(pair.Value.Values);
                    try
                    {
                        key.Complete(succeeded, items);
                    }
                    catch (Exception exception)
                    {
                        if (WorkflowExecutor.IsIrrecoverableException(exception))
                        {
                            throw;
                        }
                        WorkflowTrace.Runtime.TraceEvent(TraceEventType.Warning, 0, "Work Item {0} threw exception on complete notification", new object[] { pair.GetType() });
                    }
                }
            }

            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing && (this.Items != null))
                {
                    this.Items.Clear();
                    this.Items = null;
                }
            }

            internal bool IsDirty
            {
                get
                {
                    if (this.IsUsable)
                    {
                        foreach (KeyValuePair<IPendingWork, SortedList<long, object>> pair in this.WorkItems)
                        {
                            try
                            {
                                if (pair.Key.MustCommit(pair.Value))
                                {
                                    return true;
                                }
                            }
                            catch (Exception exception)
                            {
                                if (WorkflowExecutor.IsIrrecoverableException(exception))
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    return false;
                }
            }

            internal bool IsUsable
            {
                get
                {
                    return (this.Items != null);
                }
            }

            internal Dictionary<IPendingWork, SortedList<long, object>> WorkItems
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.Items;
                }
            }
        }
    }
}

