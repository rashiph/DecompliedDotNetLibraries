namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal static class OperationWithTimeoutComposer
    {
        public static IAsyncResult BeginComposeAsyncOperations(TimeSpan timeout, OperationWithTimeoutBeginCallback[] beginOperations, OperationEndCallback[] endOperations, AsyncCallback callback, object state)
        {
            return new ComposedAsyncResult(timeout, beginOperations, endOperations, callback, state);
        }

        public static void EndComposeAsyncOperations(IAsyncResult result)
        {
            ComposedAsyncResult.End(result);
        }

        public static TimeSpan RemainingTime(IAsyncResult result)
        {
            return ((ComposedAsyncResult) result).RemainingTime();
        }

        private class ComposedAsyncResult : AsyncResult
        {
            private OperationWithTimeoutBeginCallback[] beginOperations;
            private bool completedSynchronously;
            private int currentOperation;
            private OperationEndCallback[] endOperations;
            private static AsyncCallback onOperationCompleted = Fx.ThunkCallback(new AsyncCallback(OperationWithTimeoutComposer.ComposedAsyncResult.OnOperationCompletedStatic));
            private TimeoutHelper timeoutHelper;

            internal ComposedAsyncResult(TimeSpan timeout, OperationWithTimeoutBeginCallback[] beginOperations, OperationEndCallback[] endOperations, AsyncCallback callback, object state) : base(callback, state)
            {
                this.completedSynchronously = true;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.beginOperations = beginOperations;
                this.endOperations = endOperations;
                this.SkipToNextOperation();
                if (this.currentOperation < this.beginOperations.Length)
                {
                    this.beginOperations[this.currentOperation](this.RemainingTime(), onOperationCompleted, this);
                }
                else
                {
                    base.Complete(this.completedSynchronously);
                }
            }

            internal static void End(IAsyncResult result)
            {
                AsyncResult.End<OperationWithTimeoutComposer.ComposedAsyncResult>(result);
            }

            private void OnOperationCompleted(IAsyncResult result)
            {
                this.completedSynchronously = this.completedSynchronously && result.CompletedSynchronously;
                Exception exception = null;
                try
                {
                    this.endOperations[this.currentOperation](result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    base.Complete(this.completedSynchronously, exception);
                }
                else
                {
                    this.currentOperation++;
                    this.SkipToNextOperation();
                    if (this.currentOperation < this.beginOperations.Length)
                    {
                        try
                        {
                            this.beginOperations[this.currentOperation](this.RemainingTime(), onOperationCompleted, this);
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                            exception = exception3;
                        }
                        if (exception != null)
                        {
                            base.Complete(this.completedSynchronously, exception);
                        }
                    }
                    else
                    {
                        base.Complete(this.completedSynchronously);
                    }
                }
            }

            private static void OnOperationCompletedStatic(IAsyncResult result)
            {
                ((OperationWithTimeoutComposer.ComposedAsyncResult) result.AsyncState).OnOperationCompleted(result);
            }

            public TimeSpan RemainingTime()
            {
                return this.timeoutHelper.RemainingTime();
            }

            private void SkipToNextOperation()
            {
                while (this.currentOperation < this.beginOperations.Length)
                {
                    if (this.beginOperations[this.currentOperation] != null)
                    {
                        return;
                    }
                    this.currentOperation++;
                }
            }
        }
    }
}

