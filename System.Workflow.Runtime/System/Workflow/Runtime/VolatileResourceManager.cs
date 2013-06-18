namespace System.Workflow.Runtime
{
    using System;
    using System.Collections;
    using System.Runtime;
    using System.Transactions;

    internal sealed class VolatileResourceManager
    {
        private WorkBatch _mergedBatch;
        private WorkBatchCollection _workCollection = new WorkBatchCollection();

        internal VolatileResourceManager()
        {
        }

        internal void ClearAllBatchedWork()
        {
            if (this._workCollection != null)
            {
                this._workCollection.RollbackAllBatchedWork();
            }
        }

        internal void Commit()
        {
            this._mergedBatch = this.GetMergedBatch();
            Transaction current = Transaction.Current;
            if (null == current)
            {
                throw new InvalidOperationException(ExecutionStringManager.NullAmbientTransaction);
            }
            this._mergedBatch.Commit(current);
        }

        internal void Complete()
        {
            try
            {
                this._mergedBatch.Complete(true);
            }
            finally
            {
                if (this._mergedBatch != null)
                {
                    this._mergedBatch.Dispose();
                    this._mergedBatch = null;
                }
                if (this._workCollection != null)
                {
                    this._workCollection.ClearSubBatches();
                }
            }
        }

        private WorkBatch GetMergedBatch()
        {
            return this._workCollection.GetMergedBatch();
        }

        internal void HandleFault()
        {
            if (this._mergedBatch != null)
            {
                this._mergedBatch.Dispose();
                this._mergedBatch = null;
            }
            if (this._workCollection != null)
            {
                this._workCollection.ClearTransientBatch();
            }
        }

        internal WorkBatchCollection BatchCollection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workCollection;
            }
        }

        internal bool IsBatchDirty
        {
            get
            {
                IDictionaryEnumerator enumerator = this._workCollection.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    WorkBatch batch = (WorkBatch) enumerator.Value;
                    if (batch.IsDirty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

