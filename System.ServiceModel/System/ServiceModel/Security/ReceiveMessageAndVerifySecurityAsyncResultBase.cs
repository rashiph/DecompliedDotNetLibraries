namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Channels;

    internal abstract class ReceiveMessageAndVerifySecurityAsyncResultBase : AsyncResult
    {
        private IInputChannel innerChannel;
        private static AsyncCallback innerTryReceiveCompletedCallback = Fx.ThunkCallback(new AsyncCallback(ReceiveMessageAndVerifySecurityAsyncResultBase.InnerTryReceiveCompletedCallback));
        private Message message;
        private bool receiveCompleted;
        private TimeoutHelper timeoutHelper;

        protected ReceiveMessageAndVerifySecurityAsyncResultBase(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
        {
            this.timeoutHelper = new TimeoutHelper(timeout);
            this.innerChannel = innerChannel;
        }

        public static bool End(IAsyncResult result, out Message message)
        {
            ReceiveMessageAndVerifySecurityAsyncResultBase base2 = AsyncResult.End<ReceiveMessageAndVerifySecurityAsyncResultBase>(result);
            message = base2.message;
            return base2.receiveCompleted;
        }

        private static void InnerTryReceiveCompletedCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReceiveMessageAndVerifySecurityAsyncResultBase asyncState = (ReceiveMessageAndVerifySecurityAsyncResultBase) result.AsyncState;
                Exception exception = null;
                bool flag = false;
                try
                {
                    if (!asyncState.innerChannel.EndTryReceive(result, out asyncState.message))
                    {
                        asyncState.receiveCompleted = false;
                        flag = true;
                    }
                    else
                    {
                        asyncState.receiveCompleted = true;
                        flag = asyncState.OnInnerReceiveDone(ref asyncState.message, asyncState.timeoutHelper.RemainingTime());
                    }
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

        protected abstract bool OnInnerReceiveDone(ref Message message, TimeSpan timeout);
        public void Start()
        {
            IAsyncResult result = this.innerChannel.BeginTryReceive(this.timeoutHelper.RemainingTime(), innerTryReceiveCompletedCallback, this);
            if (result.CompletedSynchronously)
            {
                if (!this.innerChannel.EndTryReceive(result, out this.message))
                {
                    this.receiveCompleted = false;
                }
                else
                {
                    this.receiveCompleted = true;
                    if (!this.OnInnerReceiveDone(ref this.message, this.timeoutHelper.RemainingTime()))
                    {
                        return;
                    }
                }
                base.Complete(true);
            }
        }
    }
}

