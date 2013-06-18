namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    internal sealed class WorkBatchCollection : Dictionary<object, WorkBatch>
    {
        private long _workItemOrderId;
        private object mutex = new object();
        private object transientBatchID = new object();

        internal void ClearSubBatches()
        {
            lock (this.mutex)
            {
                foreach (WorkBatch batch in base.Values)
                {
                    batch.Dispose();
                }
                base.Clear();
            }
        }

        internal void ClearTransientBatch()
        {
            this.RollbackBatch(this.transientBatchID);
        }

        private WorkBatch FindBatch(object id)
        {
            WorkBatch batch = null;
            lock (this.mutex)
            {
                base.TryGetValue(id, out batch);
            }
            return batch;
        }

        internal IWorkBatch GetBatch(object id)
        {
            WorkBatch batch = null;
            lock (this.mutex)
            {
                if (base.TryGetValue(id, out batch))
                {
                    return batch;
                }
                batch = new WorkBatch(this);
                base.Add(id, batch);
            }
            return batch;
        }

        internal WorkBatch GetMergedBatch()
        {
            lock (this.mutex)
            {
                WorkBatch batch = new WorkBatch(this);
                foreach (WorkBatch batch2 in base.Values)
                {
                    batch.Merge(batch2);
                }
                return batch;
            }
        }

        internal long GetNextWorkItemOrderId(IPendingWork pendingWork)
        {
            return Interlocked.Increment(ref this._workItemOrderId);
        }

        internal IWorkBatch GetTransientBatch()
        {
            return this.GetBatch(this.transientBatchID);
        }

        internal void RollbackAllBatchedWork()
        {
            lock (this.mutex)
            {
                foreach (WorkBatch batch in base.Values)
                {
                    batch.Complete(false);
                    batch.Dispose();
                }
                base.Clear();
            }
        }

        internal void RollbackBatch(object id)
        {
            lock (this.mutex)
            {
                WorkBatch batch = this.FindBatch(id);
                if (batch != null)
                {
                    batch.Complete(false);
                    batch.Dispose();
                    base.Remove(id);
                }
            }
        }

        internal long WorkItemOrderId
        {
            get
            {
                return Interlocked.Read(ref this._workItemOrderId);
            }
            set
            {
                lock (this.mutex)
                {
                    this._workItemOrderId = value;
                }
            }
        }
    }
}

