namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Transactions;

    internal class MsmqReceiveContext : ReceiveContext
    {
        private DateTime expiryTime;
        private long lookupId;
        private MsmqReceiveContextLockManager manager;

        public MsmqReceiveContext(long lookupId, DateTime expiryTime, MsmqReceiveContextLockManager manager)
        {
            this.manager = manager;
            this.lookupId = lookupId;
            this.expiryTime = expiryTime;
        }

        public void MarkContextExpired()
        {
            base.Fault();
        }

        protected override void OnAbandon(TimeSpan timeout)
        {
            this.manager.UnlockMessage(this, timeout);
        }

        protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveContextAsyncResult.CreateAbandon(this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReceiveContextAsyncResult.CreateComplete(this, timeout, callback, state);
        }

        protected override void OnComplete(TimeSpan timeout)
        {
            this.manager.DeleteMessage(this, timeout);
        }

        protected override void OnEndAbandon(IAsyncResult result)
        {
            ReceiveContextAsyncResult.End(result);
        }

        protected override void OnEndComplete(IAsyncResult result)
        {
            ReceiveContextAsyncResult.End(result);
        }

        public DateTime ExpiryTime
        {
            get
            {
                return this.expiryTime;
            }
        }

        public long LookupId
        {
            get
            {
                return this.lookupId;
            }
        }

        public MsmqReceiveContextLockManager Manager
        {
            get
            {
                return this.manager;
            }
        }

        private class ReceiveContextAsyncResult : AsyncResult
        {
            private Transaction associatedTransaction;
            private static Action<object> onAbandon;
            private static Action<object> onComplete;
            private MsmqReceiveContext receiver;
            private TimeoutHelper timeoutHelper;

            private ReceiveContextAsyncResult(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state, Action<object> target) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.receiver = receiver;
                if (Transaction.Current != null)
                {
                    this.associatedTransaction = Transaction.Current;
                }
                ActionItem.Schedule(target, this);
            }

            public static IAsyncResult CreateAbandon(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (onAbandon == null)
                {
                    onAbandon = new Action<object>(MsmqReceiveContext.ReceiveContextAsyncResult.OnAbandon);
                }
                return new MsmqReceiveContext.ReceiveContextAsyncResult(receiver, timeout, callback, state, onAbandon);
            }

            public static IAsyncResult CreateComplete(MsmqReceiveContext receiver, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (onComplete == null)
                {
                    onComplete = new Action<object>(MsmqReceiveContext.ReceiveContextAsyncResult.OnComplete);
                }
                return new MsmqReceiveContext.ReceiveContextAsyncResult(receiver, timeout, callback, state, onComplete);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<MsmqReceiveContext.ReceiveContextAsyncResult>(result);
            }

            private static void OnAbandon(object parameter)
            {
                MsmqReceiveContext.ReceiveContextAsyncResult result = parameter as MsmqReceiveContext.ReceiveContextAsyncResult;
                Exception exception = null;
                try
                {
                    result.receiver.OnAbandon(result.timeoutHelper.RemainingTime());
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }

            private static void OnComplete(object parameter)
            {
                MsmqReceiveContext.ReceiveContextAsyncResult result = parameter as MsmqReceiveContext.ReceiveContextAsyncResult;
                Exception exception = null;
                Transaction current = null;
                try
                {
                    current = Transaction.Current;
                    Transaction.Current = result.associatedTransaction;
                    result.receiver.OnComplete(result.timeoutHelper.RemainingTime());
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                finally
                {
                    Transaction.Current = current;
                }
                result.Complete(false, exception);
            }
        }
    }
}

