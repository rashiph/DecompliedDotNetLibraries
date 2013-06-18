namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class CloseInputAsyncResult : AsyncResult
    {
        private bool completedSynchronously;
        private int count;
        private Exception exception;
        private static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(CloseInputAsyncResult.Callback));
        private TimeoutHelper timeoutHelper;

        public CloseInputAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, InstanceContext[] instances) : base(otherCallback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.completedSynchronously = true;
            this.count = instances.Length;
            if (this.count == 0)
            {
                base.Complete(true);
            }
            else
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    IAsyncResult result;
                    CallbackState state2 = new CallbackState(this, instances[i]);
                    try
                    {
                        result = instances[i].BeginCloseInput(this.timeoutHelper.RemainingTime(), nestedCallback, state2);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.Decrement(true, exception);
                        continue;
                    }
                    if (result.CompletedSynchronously)
                    {
                        instances[i].EndCloseInput(result);
                        this.Decrement(true);
                    }
                }
            }
        }

        private static void Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackState asyncState = (CallbackState) result.AsyncState;
                try
                {
                    asyncState.Instance.EndCloseInput(result);
                    asyncState.Result.Decrement(false);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.Result.Decrement(false, exception);
                }
            }
        }

        private void Decrement(bool completedSynchronously)
        {
            if (!completedSynchronously)
            {
                this.completedSynchronously = false;
            }
            if (Interlocked.Decrement(ref this.count) == 0)
            {
                if (this.exception != null)
                {
                    base.Complete(this.completedSynchronously, this.exception);
                }
                else
                {
                    base.Complete(this.completedSynchronously);
                }
            }
        }

        private void Decrement(bool completedSynchronously, Exception exception)
        {
            this.exception = exception;
            this.Decrement(completedSynchronously);
        }

        public static void End(IAsyncResult result)
        {
            AsyncResult.End<CloseInputAsyncResult>(result);
        }

        private class CallbackState
        {
            private InstanceContext instance;
            private CloseInputAsyncResult result;

            public CallbackState(CloseInputAsyncResult result, InstanceContext instance)
            {
                this.result = result;
                this.instance = instance;
            }

            public InstanceContext Instance
            {
                get
                {
                    return this.instance;
                }
            }

            public CloseInputAsyncResult Result
            {
                get
                {
                    return this.result;
                }
            }
        }
    }
}

