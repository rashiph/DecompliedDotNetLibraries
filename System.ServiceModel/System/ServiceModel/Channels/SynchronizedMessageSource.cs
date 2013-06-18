namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.Threading;

    internal class SynchronizedMessageSource
    {
        private IMessageSource source;
        private ThreadNeutralSemaphore sourceLock;

        public SynchronizedMessageSource(IMessageSource source)
        {
            this.source = source;
            this.sourceLock = new ThreadNeutralSemaphore(1);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ReceiveAsyncResult(this, timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForMessageAsyncResult(this, timeout, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            return SynchronizedAsyncResult<Message>.End(result);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return SynchronizedAsyncResult<bool>.End(result);
        }

        public Message Receive(TimeSpan timeout)
        {
            Message message;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.sourceLock.TryEnter(helper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("ReceiveTimedOut2", new object[] { timeout }), ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }
            try
            {
                message = this.source.Receive(helper.RemainingTime());
            }
            finally
            {
                this.sourceLock.Exit();
            }
            return message;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            bool flag;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.sourceLock.TryEnter(helper.RemainingTime()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("WaitForMessageTimedOut", new object[] { timeout }), ThreadNeutralSemaphore.CreateEnterTimedOutException(timeout)));
            }
            try
            {
                flag = this.source.WaitForMessage(helper.RemainingTime());
            }
            finally
            {
                this.sourceLock.Exit();
            }
            return flag;
        }

        private class ReceiveAsyncResult : SynchronizedMessageSource.SynchronizedAsyncResult<Message>
        {
            private static WaitCallback onReceiveComplete = new WaitCallback(SynchronizedMessageSource.ReceiveAsyncResult.OnReceiveComplete);

            public ReceiveAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout, AsyncCallback callback, object state) : base(syncSource, timeout, callback, state)
            {
            }

            private static void OnReceiveComplete(object state)
            {
                SynchronizedMessageSource.ReceiveAsyncResult result = (SynchronizedMessageSource.ReceiveAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.SetReturnValue(result.Source.EndReceive());
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.CompleteWithUnlock(false, exception);
            }

            protected override bool PerformOperation(TimeSpan timeout)
            {
                if (base.Source.BeginReceive(timeout, onReceiveComplete, this) == AsyncReceiveResult.Completed)
                {
                    base.SetReturnValue(base.Source.EndReceive());
                    return true;
                }
                return false;
            }
        }

        private abstract class SynchronizedAsyncResult<T> : AsyncResult
        {
            private bool exitLock;
            private static FastAsyncCallback onEnterComplete;
            private T returnValue;
            private SynchronizedMessageSource syncSource;
            private TimeoutHelper timeoutHelper;

            static SynchronizedAsyncResult()
            {
                SynchronizedMessageSource.SynchronizedAsyncResult<T>.onEnterComplete = new FastAsyncCallback(SynchronizedMessageSource.SynchronizedAsyncResult<T>.OnEnterComplete);
            }

            public SynchronizedAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.syncSource = syncSource;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (syncSource.sourceLock.EnterAsync(this.timeoutHelper.RemainingTime(), SynchronizedMessageSource.SynchronizedAsyncResult<T>.onEnterComplete, this))
                {
                    bool flag2;
                    this.exitLock = true;
                    bool flag = false;
                    try
                    {
                        flag2 = this.PerformOperation(this.timeoutHelper.RemainingTime());
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.ExitLock();
                        }
                    }
                    if (flag2)
                    {
                        this.CompleteWithUnlock(true);
                    }
                }
            }

            protected void CompleteWithUnlock(bool synchronous)
            {
                this.CompleteWithUnlock(synchronous, null);
            }

            protected void CompleteWithUnlock(bool synchronous, Exception exception)
            {
                this.ExitLock();
                base.Complete(synchronous, exception);
            }

            public static T End(IAsyncResult result)
            {
                return AsyncResult.End<SynchronizedMessageSource.SynchronizedAsyncResult<T>>(result).returnValue;
            }

            private void ExitLock()
            {
                if (this.exitLock)
                {
                    this.syncSource.sourceLock.Exit();
                    this.exitLock = false;
                }
            }

            private static void OnEnterComplete(object state, Exception asyncException)
            {
                bool flag;
                SynchronizedMessageSource.SynchronizedAsyncResult<T> result = (SynchronizedMessageSource.SynchronizedAsyncResult<T>) state;
                Exception exception = asyncException;
                if (exception != null)
                {
                    flag = true;
                }
                else
                {
                    try
                    {
                        result.exitLock = true;
                        flag = result.PerformOperation(result.timeoutHelper.RemainingTime());
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
                }
                if (flag)
                {
                    result.CompleteWithUnlock(false, exception);
                }
            }

            protected abstract bool PerformOperation(TimeSpan timeout);
            protected void SetReturnValue(T returnValue)
            {
                this.returnValue = returnValue;
            }

            protected IMessageSource Source
            {
                get
                {
                    return this.syncSource.source;
                }
            }
        }

        private class WaitForMessageAsyncResult : SynchronizedMessageSource.SynchronizedAsyncResult<bool>
        {
            private static WaitCallback onWaitForMessageComplete = new WaitCallback(SynchronizedMessageSource.WaitForMessageAsyncResult.OnWaitForMessageComplete);

            public WaitForMessageAsyncResult(SynchronizedMessageSource syncSource, TimeSpan timeout, AsyncCallback callback, object state) : base(syncSource, timeout, callback, state)
            {
            }

            private static void OnWaitForMessageComplete(object state)
            {
                SynchronizedMessageSource.WaitForMessageAsyncResult result = (SynchronizedMessageSource.WaitForMessageAsyncResult) state;
                Exception exception = null;
                try
                {
                    result.SetReturnValue(result.Source.EndWaitForMessage());
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.CompleteWithUnlock(false, exception);
            }

            protected override bool PerformOperation(TimeSpan timeout)
            {
                if (base.Source.BeginWaitForMessage(timeout, onWaitForMessageComplete, this) == AsyncReceiveResult.Completed)
                {
                    base.SetReturnValue(base.Source.EndWaitForMessage());
                    return true;
                }
                return false;
            }
        }
    }
}

