namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Transactions;

    public abstract class WorkflowCommitWorkBatchService : WorkflowRuntimeService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowCommitWorkBatchService()
        {
        }

        protected internal virtual void CommitWorkBatch(CommitWorkBatchCallback commitWorkBatchCallback)
        {
            Transaction transactionToUse = null;
            if (null == Transaction.Current)
            {
                transactionToUse = new CommittableTransaction();
            }
            else
            {
                transactionToUse = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
            }
            try
            {
                using (TransactionScope scope = new TransactionScope(transactionToUse))
                {
                    commitWorkBatchCallback();
                    scope.Complete();
                }
                CommittableTransaction transaction2 = transactionToUse as CommittableTransaction;
                if (transaction2 != null)
                {
                    transaction2.Commit();
                }
                DependentTransaction transaction3 = transactionToUse as DependentTransaction;
                if (transaction3 != null)
                {
                    transaction3.Complete();
                }
            }
            catch (Exception exception)
            {
                transactionToUse.Rollback(exception);
                throw;
            }
            finally
            {
                if (transactionToUse != null)
                {
                    transactionToUse.Dispose();
                }
            }
        }

        public delegate void CommitWorkBatchCallback();
    }
}

