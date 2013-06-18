namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal sealed class SendReceiveReliableRequestor : ReliableRequestor
    {
        private bool timeoutIsSafe;

        public override WsrmMessageInfo GetInfo()
        {
            throw Fx.AssertAndThrow("Not Supported.");
        }

        private TimeSpan GetReceiveTimeout(TimeSpan timeoutRemaining)
        {
            if ((timeoutRemaining >= ReliableMessagingConstants.RequestorReceiveTime) && this.timeoutIsSafe)
            {
                return ReliableMessagingConstants.RequestorReceiveTime;
            }
            return timeoutRemaining;
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SendReceiveAsyncResult(this, request, timeout, callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            return SendReceiveAsyncResult.End(result);
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            RequestContext context;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.Binder.Send(request, helper.RemainingTime(), MaskingMode.None);
            TimeSpan receiveTimeout = this.GetReceiveTimeout(helper.RemainingTime());
            base.Binder.TryReceive(receiveTimeout, out context, MaskingMode.None);
            if (context == null)
            {
                return null;
            }
            return context.RequestMessage;
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            throw Fx.AssertAndThrow("Not Supported.");
        }

        public bool TimeoutIsSafe
        {
            set
            {
                this.timeoutIsSafe = value;
            }
        }

        private class SendReceiveAsyncResult : AsyncResult
        {
            private Message request;
            private SendReceiveReliableRequestor requestor;
            private Message response;
            private static AsyncCallback sendCallback = Fx.ThunkCallback(new AsyncCallback(SendReceiveReliableRequestor.SendReceiveAsyncResult.SendCallback));
            private TimeoutHelper timeoutHelper;
            private static AsyncCallback tryReceiveCallback = Fx.ThunkCallback(new AsyncCallback(SendReceiveReliableRequestor.SendReceiveAsyncResult.TryReceiveCallback));

            internal SendReceiveAsyncResult(SendReceiveReliableRequestor requestor, Message request, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.requestor = requestor;
                this.request = request;
                this.timeoutHelper = new TimeoutHelper(timeout);
                if (this.BeginSend())
                {
                    base.Complete(true);
                }
            }

            private bool BeginSend()
            {
                IAsyncResult result = this.requestor.Binder.BeginSend(this.request, this.timeoutHelper.RemainingTime(), MaskingMode.None, sendCallback, this);
                return (result.CompletedSynchronously && this.EndSend(result));
            }

            public static Message End(IAsyncResult result)
            {
                return AsyncResult.End<SendReceiveReliableRequestor.SendReceiveAsyncResult>(result).response;
            }

            private bool EndSend(IAsyncResult result)
            {
                this.requestor.Binder.EndSend(result);
                TimeSpan receiveTimeout = this.requestor.GetReceiveTimeout(this.timeoutHelper.RemainingTime());
                IAsyncResult result2 = this.requestor.Binder.BeginTryReceive(receiveTimeout, MaskingMode.None, tryReceiveCallback, this);
                return (result2.CompletedSynchronously && this.EndTryReceive(result2));
            }

            private bool EndTryReceive(IAsyncResult result)
            {
                RequestContext context;
                this.requestor.Binder.EndTryReceive(result, out context);
                this.response = (context != null) ? context.RequestMessage : null;
                return true;
            }

            private static void SendCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception;
                    SendReceiveReliableRequestor.SendReceiveAsyncResult asyncState = (SendReceiveReliableRequestor.SendReceiveAsyncResult) result.AsyncState;
                    bool flag = false;
                    try
                    {
                        flag = asyncState.EndSend(result);
                        exception = null;
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

            private static void TryReceiveCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception;
                    SendReceiveReliableRequestor.SendReceiveAsyncResult asyncState = (SendReceiveReliableRequestor.SendReceiveAsyncResult) result.AsyncState;
                    bool flag = false;
                    try
                    {
                        flag = asyncState.EndTryReceive(result);
                        exception = null;
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
    }
}

