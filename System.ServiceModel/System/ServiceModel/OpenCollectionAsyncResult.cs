namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    internal class OpenCollectionAsyncResult : AsyncResult
    {
        private bool completedSynchronously;
        private int count;
        private Exception exception;
        private static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(OpenCollectionAsyncResult.Callback));
        private TimeoutHelper timeoutHelper;

        public OpenCollectionAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, IList<ICommunicationObject> collection) : base(otherCallback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.completedSynchronously = true;
            this.count = collection.Count;
            if (this.count == 0)
            {
                base.Complete(true);
            }
            else
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (this.exception != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.exception);
                    }
                    CallbackState state2 = new CallbackState(this, collection[i]);
                    IAsyncResult result = collection[i].BeginOpen(this.timeoutHelper.RemainingTime(), nestedCallback, state2);
                    if (result.CompletedSynchronously)
                    {
                        collection[i].EndOpen(result);
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
                    asyncState.Instance.EndOpen(result);
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
            AsyncResult.End<OpenCollectionAsyncResult>(result);
        }

        private class CallbackState
        {
            private ICommunicationObject instance;
            private OpenCollectionAsyncResult result;

            public CallbackState(OpenCollectionAsyncResult result, ICommunicationObject instance)
            {
                this.result = result;
                this.instance = instance;
            }

            public ICommunicationObject Instance
            {
                get
                {
                    return this.instance;
                }
            }

            public OpenCollectionAsyncResult Result
            {
                get
                {
                    return this.result;
                }
            }
        }
    }
}

