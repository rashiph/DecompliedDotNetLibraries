namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Transactions;

    internal class TransactionContext : IEnlistmentNotification
    {
        private Transaction currentTransaction;
        private WorkflowServiceInstance durableInstance;
        private static AsyncCallback handleEndPrepare = Fx.ThunkCallback(new AsyncCallback(TransactionContext.HandleEndPrepare));

        public TransactionContext(WorkflowServiceInstance durableInstance, Transaction currentTransaction)
        {
            this.currentTransaction = currentTransaction.Clone();
            this.durableInstance = durableInstance;
            this.currentTransaction.EnlistVolatile(this, EnlistmentOptions.EnlistDuringPrepareRequired);
        }

        private TransactionException GetAbortedOrInDoubtTransactionException()
        {
            try
            {
                Fx.ThrowIfTransactionAbortedOrInDoubt(this.currentTransaction);
            }
            catch (TransactionException exception)
            {
                return exception;
            }
            return null;
        }

        private static void HandleEndPrepare(IAsyncResult result)
        {
            PreparingEnlistment asyncState = (PreparingEnlistment) result.AsyncState;
            bool flag = false;
            try
            {
                if (!result.CompletedSynchronously)
                {
                    PrepareAsyncResult.End(result);
                    asyncState.Prepared();
                }
                flag = true;
            }
            catch (TransactionException)
            {
            }
            finally
            {
                if (!flag)
                {
                    asyncState.ForceRollback();
                }
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
        {
            enlistment.Done();
            this.durableInstance.TransactionCommitted();
        }

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
            TransactionException abortedOrInDoubtTransactionException = this.GetAbortedOrInDoubtTransactionException();
            this.durableInstance.OnTransactionAbortOrInDoubt(abortedOrInDoubtTransactionException);
        }

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            bool flag = false;
            try
            {
                IAsyncResult result = new PrepareAsyncResult(this, handleEndPrepare, preparingEnlistment);
                if (result.CompletedSynchronously)
                {
                    PrepareAsyncResult.End(result);
                    preparingEnlistment.Prepared();
                }
                flag = true;
            }
            catch (TransactionException)
            {
            }
            finally
            {
                if (!flag)
                {
                    preparingEnlistment.ForceRollback();
                }
            }
        }

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
        {
            enlistment.Done();
            TransactionException abortedOrInDoubtTransactionException = this.GetAbortedOrInDoubtTransactionException();
            this.durableInstance.OnTransactionAbortOrInDoubt(abortedOrInDoubtTransactionException);
        }

        public Transaction CurrentTransaction
        {
            get
            {
                return this.currentTransaction;
            }
        }

        private class PrepareAsyncResult : AsyncResult
        {
            private readonly TransactionContext context;
            private static readonly AsyncResult.AsyncCompletion onEndPersist = new AsyncResult.AsyncCompletion(TransactionContext.PrepareAsyncResult.OnEndPersist);

            public PrepareAsyncResult(TransactionContext context, AsyncCallback callback, object state) : base(callback, state)
            {
                this.context = context;
                IAsyncResult result = null;
                using (base.PrepareTransactionalCall(this.context.currentTransaction))
                {
                    result = this.context.durableInstance.BeginPersist(TimeSpan.MaxValue, base.PrepareAsyncCompletion(onEndPersist), this);
                }
                if (base.SyncContinue(result))
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<TransactionContext.PrepareAsyncResult>(result);
            }

            private static bool OnEndPersist(IAsyncResult result)
            {
                TransactionContext.PrepareAsyncResult asyncState = (TransactionContext.PrepareAsyncResult) result.AsyncState;
                asyncState.context.durableInstance.EndPersist(result);
                asyncState.context.durableInstance.OnTransactionPrepared();
                return true;
            }
        }
    }
}

