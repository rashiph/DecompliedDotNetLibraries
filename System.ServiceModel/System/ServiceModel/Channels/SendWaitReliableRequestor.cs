namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;

    internal sealed class SendWaitReliableRequestor : ReliableRequestor
    {
        private bool replied;
        private InterruptibleWaitObject replyHandle = new InterruptibleWaitObject(false, true);
        private WsrmMessageInfo replyInfo;
        private Message request;
        private object thisLock = new object();

        private IAsyncResult BeginSend(TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            try
            {
                result = base.Binder.BeginSend(this.request, timeout, MaskingMode.None, callback, state);
            }
            finally
            {
                this.request = null;
            }
            return result;
        }

        private IAsyncResult BeginWait(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeSpan waitTimeout = this.GetWaitTimeout(timeout);
            return this.replyHandle.BeginWait(waitTimeout, callback, state);
        }

        private void EndSend(IAsyncResult result)
        {
            base.Binder.EndSend(result);
        }

        private void EndWait(IAsyncResult result)
        {
            this.replyHandle.EndWait(result);
        }

        public override void Fault(CommunicationObject communicationObject)
        {
            this.replied = true;
            this.replyHandle.Fault(communicationObject);
            base.Fault(communicationObject);
        }

        public override WsrmMessageInfo GetInfo()
        {
            return this.replyInfo;
        }

        private Message GetReply(bool last)
        {
            lock (this.ThisLock)
            {
                if (this.replyInfo != null)
                {
                    this.replied = true;
                    return this.replyInfo.Message;
                }
                if (last)
                {
                    this.replied = true;
                }
            }
            return null;
        }

        private TimeSpan GetWaitTimeout(TimeSpan timeoutRemaining)
        {
            if (timeoutRemaining < ReliableMessagingConstants.RequestorReceiveTime)
            {
                return timeoutRemaining;
            }
            return ReliableMessagingConstants.RequestorReceiveTime;
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.request = request;
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.BeginSend), new OperationWithTimeoutBeginCallback(this.BeginWait) }, new OperationEndCallback[] { new OperationEndCallback(this.EndSend), new OperationEndCallback(this.EndWait) }, callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
            return this.GetReply(last);
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.Binder.Send(request, helper.RemainingTime(), MaskingMode.None);
            TimeSpan waitTimeout = this.GetWaitTimeout(helper.RemainingTime());
            this.replyHandle.Wait(waitTimeout);
            return this.GetReply(last);
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            lock (this.ThisLock)
            {
                if (this.replied || (this.replyInfo != null))
                {
                    return;
                }
                this.replyInfo = info;
            }
            this.replyHandle.Set();
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

