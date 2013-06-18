namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal class ChainedAsyncResult : AsyncResult
    {
        private static AsyncCallback begin1Callback = Fx.ThunkCallback(new AsyncCallback(ChainedAsyncResult.Begin1Callback));
        private ChainedBeginHandler begin2;
        private static AsyncCallback begin2Callback = Fx.ThunkCallback(new AsyncCallback(ChainedAsyncResult.Begin2Callback));
        private ChainedEndHandler end1;
        private ChainedEndHandler end2;
        private TimeoutHelper timeoutHelper;

        protected ChainedAsyncResult(TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
        }

        public ChainedAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ChainedBeginHandler begin1, ChainedEndHandler end1, ChainedBeginHandler begin2, ChainedEndHandler end2) : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.Begin(begin1, end1, begin2, end2);
        }

        protected void Begin(ChainedBeginHandler begin1, ChainedEndHandler end1, ChainedBeginHandler begin2, ChainedEndHandler end2)
        {
            this.end1 = end1;
            this.begin2 = begin2;
            this.end2 = end2;
            IAsyncResult result = begin1(this.timeoutHelper.RemainingTime(), begin1Callback, this);
            if (result.CompletedSynchronously && this.Begin1Completed(result))
            {
                base.Complete(true);
            }
        }

        private static void Begin1Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ChainedAsyncResult asyncState = (ChainedAsyncResult) result.AsyncState;
                bool flag = false;
                Exception exception = null;
                try
                {
                    flag = asyncState.Begin1Completed(result);
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

        private bool Begin1Completed(IAsyncResult result)
        {
            this.end1(result);
            result = this.begin2(this.timeoutHelper.RemainingTime(), begin2Callback, this);
            if (!result.CompletedSynchronously)
            {
                return false;
            }
            this.end2(result);
            return true;
        }

        private static void Begin2Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ChainedAsyncResult asyncState = (ChainedAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.end2(result);
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

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<ChainedAsyncResult>(result);
        }
    }
}

