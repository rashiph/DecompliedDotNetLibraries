namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;
    using System.Transactions;

    public abstract class ReceiveContext
    {
        private bool contextFaulted;
        public static readonly string Name = "ReceiveContext";
        private ThreadNeutralSemaphore stateLock;
        private object thisLock = new object();

        public event EventHandler Faulted;

        protected ReceiveContext()
        {
            this.State = ReceiveContextState.Received;
            this.stateLock = new ThreadNeutralSemaphore(1);
        }

        public virtual void Abandon(TimeSpan timeout)
        {
            this.Abandon(null, timeout);
        }

        public virtual void Abandon(Exception exception, TimeSpan timeout)
        {
            this.EnsureValidTimeout(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.WaitForStateLock(helper.RemainingTime());
            try
            {
                if (this.PreAbandon())
                {
                    return;
                }
            }
            finally
            {
                this.ReleaseStateLock();
            }
            bool flag = false;
            try
            {
                if (exception == null)
                {
                    this.OnAbandon(helper.RemainingTime());
                }
                else
                {
                    if (TD.ReceiveContextAbandonWithExceptionIsEnabled())
                    {
                        TD.ReceiveContextAbandonWithException(base.GetType().ToString(), exception.GetType().ToString());
                    }
                    this.OnAbandon(exception, helper.RemainingTime());
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfFaulted();
                    this.ThrowIfNotAbandoning();
                    this.State = ReceiveContextState.Abandoned;
                }
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    if (TD.ReceiveContextAbandonFailedIsEnabled())
                    {
                        TD.ReceiveContextAbandonFailed(base.GetType().ToString());
                    }
                    this.Fault();
                }
            }
        }

        public virtual IAsyncResult BeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.BeginAbandon(null, timeout, callback, state);
        }

        public virtual IAsyncResult BeginAbandon(Exception exception, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.EnsureValidTimeout(timeout);
            return new AbandonAsyncResult(this, exception, timeout, callback, state);
        }

        public virtual IAsyncResult BeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.EnsureValidTimeout(timeout);
            return new CompleteAsyncResult(this, timeout, callback, state);
        }

        public virtual void Complete(TimeSpan timeout)
        {
            this.EnsureValidTimeout(timeout);
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.WaitForStateLock(helper.RemainingTime());
            bool flag = false;
            try
            {
                this.PreComplete();
                flag = true;
            }
            finally
            {
                if (!flag || (Transaction.Current == null))
                {
                    this.ReleaseStateLock();
                }
            }
            flag = false;
            try
            {
                this.OnComplete(helper.RemainingTime());
                lock (this.ThisLock)
                {
                    this.ThrowIfFaulted();
                    this.ThrowIfNotCompleting();
                    this.State = ReceiveContextState.Completed;
                }
                flag = true;
            }
            finally
            {
                if (!flag)
                {
                    if (TD.ReceiveContextCompleteFailedIsEnabled())
                    {
                        TD.ReceiveContextCompleteFailed(base.GetType().ToString());
                    }
                    this.Fault();
                }
            }
        }

        public virtual void EndAbandon(IAsyncResult result)
        {
            AbandonAsyncResult.End(result);
        }

        public virtual void EndComplete(IAsyncResult result)
        {
            CompleteAsyncResult.End(result);
        }

        private void EnsureValidTimeout(TimeSpan timeout)
        {
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            if (TimeoutHelper.IsTooLarge(timeout))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
            }
        }

        protected internal virtual void Fault()
        {
            lock (this.ThisLock)
            {
                if (((this.State == ReceiveContextState.Completed) || (this.State == ReceiveContextState.Abandoned)) || (this.State == ReceiveContextState.Faulted))
                {
                    return;
                }
                this.State = ReceiveContextState.Faulted;
            }
            this.OnFaulted();
        }

        protected abstract void OnAbandon(TimeSpan timeout);
        protected virtual void OnAbandon(Exception exception, TimeSpan timeout)
        {
            this.OnAbandon(timeout);
        }

        protected abstract IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state);
        protected virtual IAsyncResult OnBeginAbandon(Exception exception, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.OnBeginAbandon(timeout, callback, state);
        }

        protected abstract IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void OnComplete(TimeSpan timeout);
        protected abstract void OnEndAbandon(IAsyncResult result);
        protected abstract void OnEndComplete(IAsyncResult result);
        protected virtual void OnFaulted()
        {
            lock (this.ThisLock)
            {
                if (this.contextFaulted)
                {
                    return;
                }
                this.contextFaulted = true;
            }
            if (TD.ReceiveContextFaultedIsEnabled())
            {
                TD.ReceiveContextFaulted(base.GetType().ToString());
            }
            EventHandler faulted = this.Faulted;
            if (faulted != null)
            {
                try
                {
                    faulted(this, EventArgs.Empty);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
                }
            }
        }

        private void OnTransactionStatusNotification(TransactionStatus status)
        {
            lock (this.ThisLock)
            {
                if ((status == TransactionStatus.Aborted) && ((this.State == ReceiveContextState.Completing) || (this.State == ReceiveContextState.Completed)))
                {
                    this.State = ReceiveContextState.Received;
                }
            }
            if (status != TransactionStatus.Active)
            {
                this.ReleaseStateLock();
            }
        }

        private bool PreAbandon()
        {
            lock (this.ThisLock)
            {
                if ((this.State == ReceiveContextState.Abandoning) || (this.State == ReceiveContextState.Abandoned))
                {
                    return true;
                }
                this.ThrowIfFaulted();
                this.ThrowIfNotReceived();
                this.State = ReceiveContextState.Abandoning;
            }
            return false;
        }

        private void PreComplete()
        {
            lock (this.ThisLock)
            {
                this.ThrowIfFaulted();
                this.ThrowIfNotReceived();
                if (Transaction.Current != null)
                {
                    Transaction.Current.EnlistVolatile(new EnlistmentNotifications(this), EnlistmentOptions.None);
                }
                this.State = ReceiveContextState.Completing;
            }
        }

        private void ReleaseStateLock()
        {
            this.stateLock.Exit();
        }

        private void ThrowIfFaulted()
        {
            if (this.State == ReceiveContextState.Faulted)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("ReceiveContextFaulted", new object[] { base.GetType().ToString() })));
            }
        }

        private void ThrowIfNotAbandoning()
        {
            if (this.State != ReceiveContextState.Abandoning)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ReceiveContextInInvalidState", new object[] { base.GetType().ToString(), this.State.ToString() })));
            }
        }

        private void ThrowIfNotCompleting()
        {
            if (this.State != ReceiveContextState.Completing)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ReceiveContextInInvalidState", new object[] { base.GetType().ToString(), this.State.ToString() })));
            }
        }

        private void ThrowIfNotReceived()
        {
            if (this.State != ReceiveContextState.Received)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ReceiveContextCannotBeUsed", new object[] { base.GetType().ToString(), this.State.ToString() })));
            }
        }

        public static bool TryGet(Message message, out ReceiveContext property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            return TryGet(message.Properties, out property);
        }

        public static bool TryGet(MessageProperties properties, out ReceiveContext property)
        {
            object obj2;
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }
            property = null;
            if (properties.TryGetValue(Name, out obj2))
            {
                property = (ReceiveContext) obj2;
                return true;
            }
            return false;
        }

        private void WaitForStateLock(TimeSpan timeout)
        {
            try
            {
                this.stateLock.Enter(timeout);
            }
            catch (TimeoutException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.WrapStateException(exception));
            }
        }

        private bool WaitForStateLockAsync(TimeSpan timeout, FastAsyncCallback callback, object state)
        {
            return this.stateLock.EnterAsync(timeout, callback, state);
        }

        private Exception WrapStateException(Exception exception)
        {
            return new InvalidOperationException(System.ServiceModel.SR.GetString("ReceiveContextInInvalidState", new object[] { base.GetType().ToString(), this.State.ToString() }), exception);
        }

        public ReceiveContextState State { get; protected set; }

        protected object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        private sealed class AbandonAsyncResult : ReceiveContext.WaitAndContinueOperationAsyncResult
        {
            private Exception exception;
            private static AsyncResult.AsyncCompletion handleOperationComplete = new AsyncResult.AsyncCompletion(ReceiveContext.AbandonAsyncResult.HandleOperationComplete);

            public AbandonAsyncResult(ReceiveContext receiveContext, Exception exception, TimeSpan timeout, AsyncCallback callback, object state) : base(receiveContext, timeout, callback, state)
            {
                this.exception = exception;
                base.Begin();
            }

            protected override bool ContinueOperation()
            {
                IAsyncResult result;
                try
                {
                    if (base.ReceiveContext.PreAbandon())
                    {
                        return true;
                    }
                }
                finally
                {
                    base.ReceiveContext.ReleaseStateLock();
                }
                bool flag = false;
                try
                {
                    if (this.exception == null)
                    {
                        result = base.ReceiveContext.OnBeginAbandon(base.TimeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleOperationComplete), this);
                    }
                    else
                    {
                        if (TD.ReceiveContextAbandonWithExceptionIsEnabled())
                        {
                            TD.ReceiveContextAbandonWithException(base.GetType().ToString(), this.exception.GetType().ToString());
                        }
                        result = base.ReceiveContext.OnBeginAbandon(this.exception, base.TimeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleOperationComplete), this);
                    }
                    flag = true;
                }
                finally
                {
                    if (!flag)
                    {
                        if (TD.ReceiveContextAbandonFailedIsEnabled())
                        {
                            TD.ReceiveContextAbandonFailed(base.GetType().ToString());
                        }
                        base.ReceiveContext.Fault();
                    }
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReceiveContext.AbandonAsyncResult>(result);
            }

            private void EndAbandon(IAsyncResult result)
            {
                base.ReceiveContext.OnEndAbandon(result);
                lock (base.ReceiveContext.ThisLock)
                {
                    base.ReceiveContext.ThrowIfFaulted();
                    base.ReceiveContext.ThrowIfNotAbandoning();
                    base.ReceiveContext.State = ReceiveContextState.Abandoned;
                }
            }

            private static bool HandleOperationComplete(IAsyncResult result)
            {
                bool flag2;
                bool flag = false;
                ReceiveContext.AbandonAsyncResult asyncState = (ReceiveContext.AbandonAsyncResult) result.AsyncState;
                try
                {
                    asyncState.EndAbandon(result);
                    flag = true;
                    flag2 = true;
                }
                finally
                {
                    if (!flag)
                    {
                        if (TD.ReceiveContextAbandonFailedIsEnabled())
                        {
                            TD.ReceiveContextAbandonFailed(asyncState.GetType().ToString());
                        }
                        asyncState.ReceiveContext.Fault();
                    }
                }
                return flag2;
            }
        }

        private sealed class CompleteAsyncResult : ReceiveContext.WaitAndContinueOperationAsyncResult
        {
            private static AsyncResult.AsyncCompletion handleOperationComplete = new AsyncResult.AsyncCompletion(ReceiveContext.CompleteAsyncResult.HandleOperationComplete);
            private Transaction transaction;

            public CompleteAsyncResult(ReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state) : base(receiveContext, timeout, callback, state)
            {
                this.transaction = Transaction.Current;
                base.Begin();
            }

            protected override bool ContinueOperation()
            {
                IAsyncResult result;
                using (base.PrepareTransactionalCall(this.transaction))
                {
                    bool flag = false;
                    try
                    {
                        base.ReceiveContext.PreComplete();
                        flag = true;
                    }
                    finally
                    {
                        if (!flag || (this.transaction == null))
                        {
                            base.ReceiveContext.ReleaseStateLock();
                        }
                    }
                    flag = false;
                    try
                    {
                        result = base.ReceiveContext.OnBeginComplete(base.TimeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleOperationComplete), this);
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            if (TD.ReceiveContextCompleteFailedIsEnabled())
                            {
                                TD.ReceiveContextCompleteFailed(base.GetType().ToString());
                            }
                            base.ReceiveContext.Fault();
                        }
                    }
                }
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReceiveContext.CompleteAsyncResult>(result);
            }

            private void EndComplete(IAsyncResult result)
            {
                base.ReceiveContext.OnEndComplete(result);
                lock (base.ReceiveContext.ThisLock)
                {
                    base.ReceiveContext.ThrowIfFaulted();
                    base.ReceiveContext.ThrowIfNotCompleting();
                    base.ReceiveContext.State = ReceiveContextState.Completed;
                }
            }

            private static bool HandleOperationComplete(IAsyncResult result)
            {
                bool flag2;
                ReceiveContext.CompleteAsyncResult asyncState = (ReceiveContext.CompleteAsyncResult) result.AsyncState;
                bool flag = false;
                try
                {
                    asyncState.EndComplete(result);
                    flag = true;
                    flag2 = true;
                }
                finally
                {
                    if (!flag)
                    {
                        if (TD.ReceiveContextCompleteFailedIsEnabled())
                        {
                            TD.ReceiveContextCompleteFailed(asyncState.GetType().ToString());
                        }
                        asyncState.ReceiveContext.Fault();
                    }
                }
                return flag2;
            }
        }

        private class EnlistmentNotifications : IEnlistmentNotification
        {
            private ReceiveContext context;

            public EnlistmentNotifications(ReceiveContext context)
            {
                this.context = context;
            }

            public void Commit(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Committed);
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.InDoubt);
                enlistment.Done();
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Active);
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.context.OnTransactionStatusNotification(TransactionStatus.Aborted);
                enlistment.Done();
            }
        }

        private abstract class WaitAndContinueOperationAsyncResult : AsyncResult
        {
            private static FastAsyncCallback onWaitForStateLockComplete = new FastAsyncCallback(System.ServiceModel.Channels.ReceiveContext.WaitAndContinueOperationAsyncResult.OnWaitForStateLockComplete);

            public WaitAndContinueOperationAsyncResult(System.ServiceModel.Channels.ReceiveContext receiveContext, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.ReceiveContext = receiveContext;
                this.TimeoutHelper = new System.Runtime.TimeoutHelper(timeout);
            }

            protected void Begin()
            {
                if (this.ReceiveContext.WaitForStateLockAsync(this.TimeoutHelper.RemainingTime(), onWaitForStateLockComplete, this) && this.ContinueOperation())
                {
                    base.Complete(true);
                }
            }

            protected abstract bool ContinueOperation();
            private static void OnWaitForStateLockComplete(object state, Exception asyncException)
            {
                System.ServiceModel.Channels.ReceiveContext.WaitAndContinueOperationAsyncResult result = (System.ServiceModel.Channels.ReceiveContext.WaitAndContinueOperationAsyncResult) state;
                bool flag = true;
                Exception exception = null;
                if (asyncException != null)
                {
                    if (asyncException is TimeoutException)
                    {
                        asyncException = result.ReceiveContext.WrapStateException(asyncException);
                    }
                    exception = asyncException;
                }
                else
                {
                    try
                    {
                        flag = result.ContinueOperation();
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
                if (flag)
                {
                    result.Complete(false, exception);
                }
            }

            protected System.ServiceModel.Channels.ReceiveContext ReceiveContext { get; private set; }

            protected System.Runtime.TimeoutHelper TimeoutHelper { get; private set; }
        }
    }
}

