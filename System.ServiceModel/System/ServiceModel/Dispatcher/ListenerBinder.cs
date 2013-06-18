namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal static class ListenerBinder
    {
        internal static IListenerBinder GetBinder(IChannelListener listener, MessageVersion messageVersion)
        {
            IChannelListener<IInputChannel> listener2 = listener as IChannelListener<IInputChannel>;
            if (listener2 != null)
            {
                return new InputListenerBinder(listener2, messageVersion);
            }
            IChannelListener<IInputSessionChannel> listener3 = listener as IChannelListener<IInputSessionChannel>;
            if (listener3 != null)
            {
                return new InputSessionListenerBinder(listener3, messageVersion);
            }
            IChannelListener<IReplyChannel> listener4 = listener as IChannelListener<IReplyChannel>;
            if (listener4 != null)
            {
                return new ReplyListenerBinder(listener4, messageVersion);
            }
            IChannelListener<IReplySessionChannel> listener5 = listener as IChannelListener<IReplySessionChannel>;
            if (listener5 != null)
            {
                return new ReplySessionListenerBinder(listener5, messageVersion);
            }
            IChannelListener<IDuplexChannel> listener6 = listener as IChannelListener<IDuplexChannel>;
            if (listener6 != null)
            {
                return new DuplexListenerBinder(listener6, messageVersion);
            }
            IChannelListener<IDuplexSessionChannel> listener7 = listener as IChannelListener<IDuplexSessionChannel>;
            if (listener7 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnknownListenerType1", new object[] { listener.Uri.AbsoluteUri })));
            }
            return new DuplexSessionListenerBinder(listener7, messageVersion);
        }

        private class DuplexListenerBinder : IListenerBinder
        {
            private IRequestReplyCorrelator correlator = new RequestReplyCorrelator();
            private IChannelListener<IDuplexChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal DuplexListenerBinder(IChannelListener<IDuplexChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IDuplexChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IDuplexChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }

        private class DuplexSessionListenerBinder : IListenerBinder
        {
            private IRequestReplyCorrelator correlator = new RequestReplyCorrelator();
            private IChannelListener<IDuplexSessionChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal DuplexSessionListenerBinder(IChannelListener<IDuplexSessionChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IDuplexSessionChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IDuplexSessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new DuplexChannelBinder(channel, this.correlator, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }

        private class InputListenerBinder : IListenerBinder
        {
            private IChannelListener<IInputChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal InputListenerBinder(IChannelListener<IInputChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IInputChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IInputChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }

        private class InputSessionListenerBinder : IListenerBinder
        {
            private IChannelListener<IInputSessionChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal InputSessionListenerBinder(IChannelListener<IInputSessionChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IInputSessionChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IInputSessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new InputChannelBinder(channel, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }

        private class ReplyListenerBinder : IListenerBinder
        {
            private IChannelListener<IReplyChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal ReplyListenerBinder(IChannelListener<IReplyChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IReplyChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new ReplyChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IReplyChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new ReplyChannelBinder(channel, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }

        private class ReplySessionListenerBinder : IListenerBinder
        {
            private IChannelListener<IReplySessionChannel> listener;
            private System.ServiceModel.Channels.MessageVersion messageVersion;

            internal ReplySessionListenerBinder(IChannelListener<IReplySessionChannel> listener, System.ServiceModel.Channels.MessageVersion messageVersion)
            {
                this.listener = listener;
                this.messageVersion = messageVersion;
            }

            public IChannelBinder Accept(TimeSpan timeout)
            {
                IReplySessionChannel channel = this.listener.AcceptChannel(timeout);
                if (channel == null)
                {
                    return null;
                }
                return new ReplyChannelBinder(channel, this.listener.Uri);
            }

            public IAsyncResult BeginAccept(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.listener.BeginAcceptChannel(timeout, callback, state);
            }

            public IChannelBinder EndAccept(IAsyncResult result)
            {
                IReplySessionChannel channel = this.listener.EndAcceptChannel(result);
                if (channel == null)
                {
                    return null;
                }
                return new ReplyChannelBinder(channel, this.listener.Uri);
            }

            public IChannelListener Listener
            {
                get
                {
                    return this.listener;
                }
            }

            public System.ServiceModel.Channels.MessageVersion MessageVersion
            {
                get
                {
                    return this.messageVersion;
                }
            }
        }
    }
}

