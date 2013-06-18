namespace System.ServiceModel.Channels
{
    using System;

    internal class BufferedRequestContext : RequestContext
    {
        private bool delayClose;
        private RequestContext innerRequestContext;
        private object thisLock;

        public BufferedRequestContext(RequestContext requestContext)
        {
            this.innerRequestContext = requestContext;
            this.thisLock = new object();
        }

        public override void Abort()
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Abort();
        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            return this.innerRequestContext.BeginReply(message, callback, state);
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerRequestContext.BeginReply(message, timeout, callback, state);
        }

        public override void Close()
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Close();
        }

        public override void Close(TimeSpan timeout)
        {
            lock (this.thisLock)
            {
                if (this.delayClose)
                {
                    this.delayClose = false;
                    return;
                }
            }
            this.innerRequestContext.Close(timeout);
        }

        public void DelayClose(bool delay)
        {
            lock (this.thisLock)
            {
                this.delayClose = delay;
            }
        }

        public override void EndReply(IAsyncResult result)
        {
            this.innerRequestContext.EndReply(result);
        }

        public void ReInitialize(Message requestMessage)
        {
            RequestContextBase innerRequestContext = this.innerRequestContext as RequestContextBase;
            if (innerRequestContext != null)
            {
                innerRequestContext.ReInitialize(requestMessage);
            }
        }

        public override void Reply(Message message)
        {
            this.innerRequestContext.Reply(message);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            this.innerRequestContext.Reply(message, timeout);
        }

        public RequestContext InnerRequestContext
        {
            get
            {
                return this.innerRequestContext;
            }
        }

        public override Message RequestMessage
        {
            get
            {
                return this.innerRequestContext.RequestMessage;
            }
        }
    }
}

