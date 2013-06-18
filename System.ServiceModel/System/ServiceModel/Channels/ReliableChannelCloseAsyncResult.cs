namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal class ReliableChannelCloseAsyncResult : AsyncResult
    {
        private IReliableChannelBinder binder;
        private static AsyncCallback onBinderCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelCloseAsyncResult.OnBinderCloseComplete));
        private static AsyncCallback onComposeAsyncOperationsComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelCloseAsyncResult.OnComposeAsyncOperationsComplete));
        private TimeoutHelper timeoutHelper;

        public ReliableChannelCloseAsyncResult(OperationWithTimeoutBeginCallback[] beginCallbacks, OperationEndCallback[] endCallbacks, IReliableChannelBinder binder, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.binder = binder;
            this.timeoutHelper = new TimeoutHelper(timeout);
            IAsyncResult result = OperationWithTimeoutComposer.BeginComposeAsyncOperations(this.timeoutHelper.RemainingTime(), beginCallbacks, endCallbacks, onComposeAsyncOperationsComplete, this);
            if (result.CompletedSynchronously && this.CompleteComposeAsyncOperations(result))
            {
                base.Complete(true);
            }
        }

        private bool CompleteComposeAsyncOperations(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            result = this.binder.BeginClose(this.timeoutHelper.RemainingTime(), MaskingMode.Handled, onBinderCloseComplete, this);
            if (result.CompletedSynchronously)
            {
                this.binder.EndClose(result);
                return true;
            }
            return false;
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ReliableChannelCloseAsyncResult>(result);
        }

        private static void OnBinderCloseComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelCloseAsyncResult asyncState = (ReliableChannelCloseAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.binder.EndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                asyncState.Complete(false, exception);
            }
        }

        private static void OnComposeAsyncOperationsComplete(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelCloseAsyncResult asyncState = (ReliableChannelCloseAsyncResult) result.AsyncState;
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = asyncState.CompleteComposeAsyncOperations(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (flag || (exception != null))
                {
                    asyncState.Complete(false, exception);
                }
            }
        }
    }
}

