namespace System.ServiceModel.Activities.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Activities;
    using System.Transactions;

    internal sealed class TransactionWaitAsyncResult : AsyncResult
    {
        private DependentTransaction dependentTransaction;
        private object thisLock;
        private IOThreadTimer timer;
        private static Action<object> timerCallback;

        internal TransactionWaitAsyncResult(System.Transactions.Transaction transaction, System.ServiceModel.Activities.Dispatcher.PersistenceContext persistenceContext, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            bool flag = false;
            TransactionException exception = null;
            this.PersistenceContext = persistenceContext;
            this.thisLock = new object();
            if (null != transaction)
            {
                this.dependentTransaction = transaction.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
            }
            else
            {
                this.dependentTransaction = null;
            }
            lock (this.ThisLock)
            {
                if (persistenceContext.QueueForTransactionLock(transaction, this))
                {
                    if (null != transaction)
                    {
                        this.dependentTransaction.Complete();
                        exception = this.CreateVolatileEnlistment(transaction);
                    }
                    flag = true;
                }
                else if (timeout != TimeSpan.MaxValue)
                {
                    this.timer = new IOThreadTimer(TimeoutCallbackAction, this, true);
                    this.timer.Set(timeout);
                }
            }
            if (flag)
            {
                base.Complete(true, exception);
            }
        }

        internal bool Complete()
        {
            Exception exception = null;
            lock (this.ThisLock)
            {
                if ((this.timer != null) && !this.timer.Cancel())
                {
                    return false;
                }
                if (this.dependentTransaction != null)
                {
                    exception = this.CreateVolatileEnlistment(this.dependentTransaction);
                    this.dependentTransaction.Complete();
                }
            }
            base.Complete(false, exception);
            return true;
        }

        private TransactionException CreateVolatileEnlistment(System.Transactions.Transaction transactionToEnlist)
        {
            TransactionException exception = null;
            PersistenceContextEnlistment enlistment = null;
            int hashCode = transactionToEnlist.GetHashCode();
            lock (System.ServiceModel.Activities.Dispatcher.PersistenceContext.Enlistments)
            {
                try
                {
                    if (!System.ServiceModel.Activities.Dispatcher.PersistenceContext.Enlistments.TryGetValue(hashCode, out enlistment))
                    {
                        enlistment = new PersistenceContextEnlistment(this.PersistenceContext, transactionToEnlist);
                        transactionToEnlist.EnlistVolatile(enlistment, EnlistmentOptions.None);
                        System.ServiceModel.Activities.Dispatcher.PersistenceContext.Enlistments.Add(hashCode, enlistment);
                        return exception;
                    }
                    enlistment.AddToEnlistment(this.PersistenceContext);
                    return exception;
                }
                catch (TransactionException exception2)
                {
                    exception = exception2;
                    this.PersistenceContext.ScheduleNextTransactionWaiter();
                }
            }
            return exception;
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<TransactionWaitAsyncResult>(result);
        }

        private static void TimeoutCallback(object state)
        {
            TransactionWaitAsyncResult result = (TransactionWaitAsyncResult) state;
            if (result.dependentTransaction != null)
            {
                result.dependentTransaction.Complete();
            }
            result.Complete(false, new TimeoutException(System.ServiceModel.Activities.SR.TransactionPersistenceTimeout));
        }

        internal System.ServiceModel.Activities.Dispatcher.PersistenceContext PersistenceContext { get; set; }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal static Action<object> TimeoutCallbackAction
        {
            get
            {
                if (timerCallback == null)
                {
                    timerCallback = new Action<object>(TransactionWaitAsyncResult.TimeoutCallback);
                }
                return timerCallback;
            }
        }

        internal System.Transactions.Transaction Transaction
        {
            get
            {
                return this.dependentTransaction;
            }
        }
    }
}

