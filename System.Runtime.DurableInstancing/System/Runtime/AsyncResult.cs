namespace System.Runtime
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;

    internal abstract class AsyncResult : IAsyncResult
    {
        private static AsyncCallback asyncCompletionWrapperCallback;
        private AsyncCallback callback;
        private bool completedSynchronously;
        private IAsyncResult deferredTransactionalResult;
        private bool endCalled;
        private Exception exception;
        private bool isCompleted;
        private ManualResetEvent manualResetEvent;
        private AsyncCompletion nextAsyncCompletion;
        private object state;
        private object thisLock;
        private TransactionSignalScope transactionContext;

        protected AsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.thisLock = new object();
        }

        private static void AsyncCompletionWrapperCallback(IAsyncResult result)
        {
            if (result == null)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InvalidNullAsyncResult));
            }
            if (!result.CompletedSynchronously)
            {
                AsyncResult asyncState = (AsyncResult) result.AsyncState;
                if ((asyncState.transactionContext == null) || asyncState.transactionContext.Signal(result))
                {
                    AsyncCompletion nextCompletion = asyncState.GetNextCompletion();
                    if (nextCompletion == null)
                    {
                        ThrowInvalidAsyncResult(result);
                    }
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        flag = nextCompletion(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        flag = true;
                        exception = exception2;
                    }
                    if (flag)
                    {
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }

        protected bool CheckSyncContinue(IAsyncResult result)
        {
            AsyncCompletion completion;
            return this.TryContinueHelper(result, out completion);
        }

        protected void Complete(bool completedSynchronously)
        {
            if (this.isCompleted)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.AsyncResultCompletedTwice(base.GetType())));
            }
            this.completedSynchronously = completedSynchronously;
            if (this.OnCompleting != null)
            {
                try
                {
                    this.OnCompleting(this, this.exception);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.exception = exception;
                }
            }
            if (completedSynchronously)
            {
                this.isCompleted = true;
            }
            else
            {
                lock (this.ThisLock)
                {
                    this.isCompleted = true;
                    if (this.manualResetEvent != null)
                    {
                        this.manualResetEvent.Set();
                    }
                }
            }
            if (this.callback != null)
            {
                try
                {
                    if (this.VirtualCallback != null)
                    {
                        this.VirtualCallback(this.callback, this);
                    }
                    else
                    {
                        this.callback(this);
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.AsyncCallbackThrewException, exception2));
                }
            }
        }

        protected void Complete(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Complete(completedSynchronously);
        }

        protected static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult: AsyncResult
        {
            if (result == null)
            {
                throw Fx.Exception.ArgumentNull("result");
            }
            TAsyncResult local = result as TAsyncResult;
            if (local == null)
            {
                throw Fx.Exception.Argument("result", SRCore.InvalidAsyncResult);
            }
            if (local.endCalled)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.AsyncResultAlreadyEnded));
            }
            local.endCalled = true;
            if (!local.isCompleted)
            {
                local.AsyncWaitHandle.WaitOne();
            }
            if (local.manualResetEvent != null)
            {
                local.manualResetEvent.Close();
            }
            if (local.exception != null)
            {
                throw Fx.Exception.AsError(local.exception);
            }
            return local;
        }

        private AsyncCompletion GetNextCompletion()
        {
            AsyncCompletion nextAsyncCompletion = this.nextAsyncCompletion;
            this.transactionContext = null;
            this.nextAsyncCompletion = null;
            return nextAsyncCompletion;
        }

        protected AsyncCallback PrepareAsyncCompletion(AsyncCompletion callback)
        {
            if (this.transactionContext != null)
            {
                if (this.transactionContext.IsPotentiallyAbandoned)
                {
                    this.transactionContext = null;
                }
                else
                {
                    this.transactionContext.Prepared();
                }
            }
            this.nextAsyncCompletion = callback;
            if (asyncCompletionWrapperCallback == null)
            {
                asyncCompletionWrapperCallback = Fx.ThunkCallback(new AsyncCallback(AsyncResult.AsyncCompletionWrapperCallback));
            }
            return asyncCompletionWrapperCallback;
        }

        protected IDisposable PrepareTransactionalCall(Transaction transaction)
        {
            if ((this.transactionContext != null) && !this.transactionContext.IsPotentiallyAbandoned)
            {
                ThrowInvalidAsyncResult("PrepareTransactionalCall should only be called as the object of non-nested using statements. If the Begin succeeds, Check/SyncContinue must be called before another PrepareTransactionalCall.");
            }
            return (this.transactionContext = (transaction == null) ? null : new TransactionSignalScope(this, transaction));
        }

        protected bool SyncContinue(IAsyncResult result)
        {
            AsyncCompletion completion;
            return (this.TryContinueHelper(result, out completion) && completion(result));
        }

        private static void ThrowInvalidAsyncResult(IAsyncResult result)
        {
            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InvalidAsyncResultImplementation(result.GetType())));
        }

        private static void ThrowInvalidAsyncResult(string debugText)
        {
            string invalidAsyncResultImplementationGeneric = SRCore.InvalidAsyncResultImplementationGeneric;
            throw Fx.Exception.AsError(new InvalidOperationException(invalidAsyncResultImplementationGeneric));
        }

        private bool TryContinueHelper(IAsyncResult result, out AsyncCompletion callback)
        {
            if (result == null)
            {
                throw Fx.Exception.AsError(new InvalidOperationException(SRCore.InvalidNullAsyncResult));
            }
            callback = null;
            if (result.CompletedSynchronously)
            {
                if (this.transactionContext != null)
                {
                    if (this.transactionContext.State != TransactionSignalState.Completed)
                    {
                        ThrowInvalidAsyncResult("Check/SyncContinue cannot be called from within the PrepareTransactionalCall using block.");
                    }
                    else if (this.transactionContext.IsSignalled)
                    {
                        ThrowInvalidAsyncResult(result);
                    }
                }
            }
            else
            {
                if (!object.ReferenceEquals(result, this.deferredTransactionalResult))
                {
                    return false;
                }
                if ((this.transactionContext == null) || !this.transactionContext.IsSignalled)
                {
                    ThrowInvalidAsyncResult(result);
                }
                this.deferredTransactionalResult = null;
            }
            callback = this.GetNextCompletion();
            if (callback == null)
            {
                ThrowInvalidAsyncResult("Only call Check/SyncContinue once per async operation (once per PrepareAsyncCompletion).");
            }
            return true;
        }

        public object AsyncState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.state;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.manualResetEvent == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.manualResetEvent == null)
                        {
                            this.manualResetEvent = new ManualResetEvent(this.isCompleted);
                        }
                    }
                }
                return this.manualResetEvent;
            }
        }

        public bool CompletedSynchronously
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completedSynchronously;
            }
        }

        public bool HasCallback
        {
            get
            {
                return (this.callback != null);
            }
        }

        public bool IsCompleted
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.isCompleted;
            }
        }

        protected Action<AsyncResult, Exception> OnCompleting
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<OnCompleting>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<OnCompleting>k__BackingField = value;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        protected Action<AsyncCallback, IAsyncResult> VirtualCallback
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<VirtualCallback>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<VirtualCallback>k__BackingField = value;
            }
        }

        protected delegate bool AsyncCompletion(IAsyncResult result);

        private class TransactionSignalScope : SignalGate<IAsyncResult>, IDisposable
        {
            private AsyncResult parent;
            private TransactionScope transactionScope;

            public TransactionSignalScope(AsyncResult result, Transaction transaction)
            {
                this.parent = result;
                this.transactionScope = Fx.CreateTransactionScope(transaction);
            }

            public void Prepared()
            {
                if (this.State != AsyncResult.TransactionSignalState.Ready)
                {
                    AsyncResult.ThrowInvalidAsyncResult("PrepareAsyncCompletion should only be called once per PrepareTransactionalCall.");
                }
                this.State = AsyncResult.TransactionSignalState.Prepared;
            }

            void IDisposable.Dispose()
            {
                IAsyncResult result;
                if (this.State == AsyncResult.TransactionSignalState.Ready)
                {
                    this.State = AsyncResult.TransactionSignalState.Abandoned;
                }
                else if (this.State == AsyncResult.TransactionSignalState.Prepared)
                {
                    this.State = AsyncResult.TransactionSignalState.Completed;
                }
                else
                {
                    AsyncResult.ThrowInvalidAsyncResult("PrepareTransactionalCall should only be called in a using. Dispose called multiple times.");
                }
                try
                {
                    Fx.CompleteTransactionScope(ref this.transactionScope);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new InvalidOperationException(SRCore.AsyncTransactionException));
                }
                if ((this.State == AsyncResult.TransactionSignalState.Completed) && base.Unlock(out result))
                {
                    if (this.parent.deferredTransactionalResult != null)
                    {
                        AsyncResult.ThrowInvalidAsyncResult(this.parent.deferredTransactionalResult);
                    }
                    this.parent.deferredTransactionalResult = result;
                }
            }

            public bool IsPotentiallyAbandoned
            {
                get
                {
                    return ((this.State == AsyncResult.TransactionSignalState.Abandoned) || ((this.State == AsyncResult.TransactionSignalState.Completed) && !base.IsSignalled));
                }
            }

            public AsyncResult.TransactionSignalState State
            {
                [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.<State>k__BackingField;
                }
                [CompilerGenerated]
                private set
                {
                    this.<State>k__BackingField = value;
                }
            }
        }

        private enum TransactionSignalState
        {
            Ready,
            Prepared,
            Completed,
            Abandoned
        }
    }
}

