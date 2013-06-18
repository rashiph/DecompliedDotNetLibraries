namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Threading;

    internal class CloseCollectionAsyncResult : AsyncResult
    {
        private bool completedSynchronously;
        private int count;
        private Exception exception;
        private static AsyncCallback nestedCallback = Fx.ThunkCallback(new AsyncCallback(CloseCollectionAsyncResult.Callback));

        public CloseCollectionAsyncResult(TimeSpan timeout, AsyncCallback otherCallback, object state, IList<ICommunicationObject> collection) : base(otherCallback, state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
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
                    IAsyncResult result;
                    CallbackState state2 = new CallbackState(this, collection[i]);
                    try
                    {
                        result = collection[i].BeginClose(helper.RemainingTime(), nestedCallback, state2);
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.Decrement(true, exception);
                        collection[i].Abort();
                        continue;
                    }
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteClose(collection[i], result);
                    }
                }
            }
        }

        private static void Callback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CallbackState asyncState = (CallbackState) result.AsyncState;
                asyncState.Result.CompleteClose(asyncState.Instance, result);
            }
        }

        private void CompleteClose(ICommunicationObject communicationObject, IAsyncResult result)
        {
            Exception exception = null;
            try
            {
                communicationObject.EndClose(result);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                exception = exception2;
                communicationObject.Abort();
            }
            this.Decrement(result.CompletedSynchronously, exception);
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
            AsyncResult.End<CloseCollectionAsyncResult>(result);
        }

        private class CallbackState
        {
            private ICommunicationObject instance;
            private CloseCollectionAsyncResult result;

            public CallbackState(CloseCollectionAsyncResult result, ICommunicationObject instance)
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

            public CloseCollectionAsyncResult Result
            {
                get
                {
                    return this.result;
                }
            }
        }
    }
}

