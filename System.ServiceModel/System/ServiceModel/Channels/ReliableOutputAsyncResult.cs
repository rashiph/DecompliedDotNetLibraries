namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal abstract class ReliableOutputAsyncResult : AsyncResult
    {
        private IReliableChannelBinder binder;
        private Exception handledException;
        private System.ServiceModel.Channels.MaskingMode maskingMode;
        private System.ServiceModel.Channels.MessageAttemptInfo messageAttemptInfo;
        private static AsyncCallback operationCallback = Fx.ThunkCallback(new AsyncCallback(ReliableOutputAsyncResult.OperationCallback));
        private bool saveHandledException;

        protected ReliableOutputAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
        }

        public void Begin(TimeSpan timeout)
        {
            bool flag;
            if (this.saveHandledException)
            {
                flag = this.BeginInternal(timeout);
            }
            else
            {
                try
                {
                    flag = this.BeginInternal(timeout);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception) || !this.HandleException(exception))
                    {
                        throw;
                    }
                    flag = true;
                }
            }
            if (flag)
            {
                base.Complete(true);
            }
        }

        private bool BeginInternal(TimeSpan timeout)
        {
            bool flag2;
            bool flag = true;
            try
            {
                IAsyncResult result = this.BeginOperation(timeout, operationCallback, this);
                if (result.CompletedSynchronously)
                {
                    this.EndOperation(result);
                    return true;
                }
                flag = false;
                flag2 = false;
            }
            finally
            {
                if (flag)
                {
                    this.Message.Close();
                }
            }
            return flag2;
        }

        protected abstract IAsyncResult BeginOperation(TimeSpan timeout, AsyncCallback callback, object state);
        protected abstract void EndOperation(IAsyncResult result);
        private bool HandleException(Exception e)
        {
            if (this.saveHandledException && this.Binder.IsHandleable(e))
            {
                this.handledException = e;
                return true;
            }
            return false;
        }

        private static void OperationCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableOutputAsyncResult asyncState = (ReliableOutputAsyncResult) result.AsyncState;
                Exception exception = null;
                try
                {
                    asyncState.EndOperation(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    if (!asyncState.HandleException(exception2))
                    {
                        exception = exception2;
                    }
                }
                finally
                {
                    asyncState.Message.Close();
                }
                asyncState.Complete(false, exception);
            }
        }

        public IReliableChannelBinder Binder
        {
            protected get
            {
                return this.binder;
            }
            set
            {
                this.binder = value;
            }
        }

        protected Exception HandledException
        {
            get
            {
                return this.handledException;
            }
        }

        public System.ServiceModel.Channels.MaskingMode MaskingMode
        {
            get
            {
                return this.maskingMode;
            }
            set
            {
                this.maskingMode = value;
            }
        }

        public System.ServiceModel.Channels.Message Message
        {
            protected get
            {
                return this.messageAttemptInfo.Message;
            }
            set
            {
                this.messageAttemptInfo = new System.ServiceModel.Channels.MessageAttemptInfo(value, 0L, 0, null);
            }
        }

        public System.ServiceModel.Channels.MessageAttemptInfo MessageAttemptInfo
        {
            get
            {
                return this.messageAttemptInfo;
            }
            set
            {
                this.messageAttemptInfo = value;
            }
        }

        public bool SaveHandledException
        {
            set
            {
                this.saveHandledException = value;
            }
        }
    }
}

