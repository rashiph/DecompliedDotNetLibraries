namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal abstract class ReplyOverDuplexChannelBase<TInnerChannel> : LayeredChannel<TInnerChannel>, IReplyChannel, IChannel, ICommunicationObject where TInnerChannel: class, IDuplexChannel
    {
        public ReplyOverDuplexChannelBase(ChannelManagerBase channelManager, TInnerChannel innerChannel) : base(channelManager, innerChannel)
        {
        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            return this.BeginReceiveRequest(base.DefaultReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginTryReceive(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.InnerChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            return ReplyChannel.HelpEndReceiveRequest(result);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            Message message;
            if (!base.InnerChannel.EndTryReceive(result, out message))
            {
                context = null;
                return false;
            }
            context = this.WrapInnerMessage(message);
            return true;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            return base.InnerChannel.EndWaitForMessage(result);
        }

        public RequestContext ReceiveRequest()
        {
            return this.ReceiveRequest(base.DefaultReceiveTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            return ReplyChannel.HelpReceiveRequest(this, timeout);
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            Message message;
            if (!base.InnerChannel.TryReceive(timeout, out message))
            {
                context = null;
                return false;
            }
            context = this.WrapInnerMessage(message);
            return true;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            return base.InnerChannel.WaitForMessage(timeout);
        }

        private RequestContext WrapInnerMessage(Message message)
        {
            if (message == null)
            {
                return null;
            }
            return new DuplexRequestContext<TInnerChannel>(message, base.Manager, base.InnerChannel);
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return base.InnerChannel.LocalAddress;
            }
        }

        private class DuplexRequestContext : RequestContext
        {
            private IDefaultCommunicationTimeouts defaultTimeouts;
            private bool disposed;
            private IDuplexChannel innerChannel;
            private EndpointAddress replyTo;
            private Message request;
            private object thisLock;

            public DuplexRequestContext(Message request, IDefaultCommunicationTimeouts defaultTimeouts, IDuplexChannel innerChannel)
            {
                this.request = request;
                this.defaultTimeouts = defaultTimeouts;
                this.innerChannel = innerChannel;
                if (request != null)
                {
                    this.replyTo = request.Headers.ReplyTo;
                }
                this.thisLock = new object();
            }

            public override void Abort()
            {
                this.Dispose(true);
            }

            public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
            {
                return this.BeginReply(message, this.defaultTimeouts.SendTimeout, callback, state);
            }

            public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.PrepareReply(message);
                return this.innerChannel.BeginSend(message, timeout, callback, state);
            }

            public override void Close()
            {
                this.Close(this.defaultTimeouts.CloseTimeout);
            }

            public override void Close(TimeSpan timeout)
            {
                this.Dispose(true);
            }

            protected override void Dispose(bool disposing)
            {
                bool flag = false;
                lock (this.thisLock)
                {
                    if (!this.disposed)
                    {
                        this.disposed = true;
                        flag = true;
                    }
                }
                if (flag && (this.request != null))
                {
                    this.request.Close();
                }
            }

            public override void EndReply(IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            private void PrepareReply(Message message)
            {
                if (this.replyTo != null)
                {
                    this.replyTo.ApplyTo(message);
                }
            }

            public override void Reply(Message message)
            {
                this.Reply(message, this.defaultTimeouts.SendTimeout);
            }

            public override void Reply(Message message, TimeSpan timeout)
            {
                this.PrepareReply(message);
                this.innerChannel.Send(message);
            }

            public override Message RequestMessage
            {
                get
                {
                    return this.request;
                }
            }
        }
    }
}

