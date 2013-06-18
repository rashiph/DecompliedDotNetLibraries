namespace System.ServiceModel.Channels
{
    using System;

    internal sealed class RequestReliableRequestor : ReliableRequestor
    {
        private bool replied;
        private WsrmMessageInfo replyInfo;
        private object thisLock = new object();

        public override WsrmMessageInfo GetInfo()
        {
            return this.replyInfo;
        }

        private Message GetReply(Message reply, bool last)
        {
            lock (this.ThisLock)
            {
                if ((reply != null) && (this.replyInfo != null))
                {
                    this.replyInfo = null;
                }
                else if ((reply == null) && (this.replyInfo != null))
                {
                    reply = this.replyInfo.Message;
                }
                if ((reply == null) && !last)
                {
                    return reply;
                }
                this.replied = true;
            }
            return reply;
        }

        protected override IAsyncResult OnBeginRequest(Message request, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.ClientBinder.BeginRequest(request, timeout, MaskingMode.None, callback, state);
        }

        protected override Message OnEndRequest(bool last, IAsyncResult result)
        {
            return this.GetReply(this.ClientBinder.EndRequest(result), last);
        }

        protected override Message OnRequest(Message request, TimeSpan timeout, bool last)
        {
            return this.GetReply(this.ClientBinder.Request(request, timeout, MaskingMode.None), last);
        }

        public override void SetInfo(WsrmMessageInfo info)
        {
            lock (this.ThisLock)
            {
                if (!this.replied && (this.replyInfo == null))
                {
                    this.replyInfo = info;
                }
            }
        }

        private IClientReliableChannelBinder ClientBinder
        {
            get
            {
                return (IClientReliableChannelBinder) base.Binder;
            }
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

