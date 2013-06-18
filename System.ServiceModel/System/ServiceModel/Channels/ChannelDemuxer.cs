namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;

    internal class ChannelDemuxer
    {
        private TypedChannelDemuxer inputDemuxer;
        private int maxPendingSessions = 10;
        private TimeSpan peekTimeout = UseDefaultReceiveTimeout;
        private TypedChannelDemuxer replyDemuxer;
        private Dictionary<System.Type, TypedChannelDemuxer> typeDemuxers = new Dictionary<System.Type, TypedChannelDemuxer>();
        public static readonly TimeSpan UseDefaultReceiveTimeout = TimeSpan.MinValue;

        public IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            return this.BuildChannelListener<TChannel>(context, new ChannelDemuxerFilter(new MatchAllMessageFilter(), 0));
        }

        public IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context, ChannelDemuxerFilter filter) where TChannel: class, IChannel
        {
            return this.GetTypedDemuxer(typeof(TChannel), context).BuildChannelListener<TChannel>(filter);
        }

        private TypedChannelDemuxer CreateTypedDemuxer(System.Type channelType, BindingContext context)
        {
            if (channelType == typeof(IDuplexChannel))
            {
                return (TypedChannelDemuxer) new DuplexChannelDemuxer(context);
            }
            if (channelType == typeof(IInputSessionChannel))
            {
                return (TypedChannelDemuxer) new InputSessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
            }
            if (channelType == typeof(IReplySessionChannel))
            {
                return (TypedChannelDemuxer) new ReplySessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
            }
            if (channelType != typeof(IDuplexSessionChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }
            return (TypedChannelDemuxer) new DuplexSessionChannelDemuxer(context, this.peekTimeout, this.maxPendingSessions);
        }

        private TypedChannelDemuxer GetTypedDemuxer(System.Type channelType, BindingContext context)
        {
            TypedChannelDemuxer inputDemuxer = null;
            bool flag = false;
            if (channelType == typeof(IInputChannel))
            {
                if (this.inputDemuxer == null)
                {
                    if (context.CanBuildInnerChannelListener<IReplyChannel>())
                    {
                        this.inputDemuxer = this.replyDemuxer = new ReplyChannelDemuxer(context);
                    }
                    else
                    {
                        this.inputDemuxer = new InputChannelDemuxer(context);
                    }
                    flag = true;
                }
                inputDemuxer = this.inputDemuxer;
            }
            else if (channelType == typeof(IReplyChannel))
            {
                if (this.replyDemuxer == null)
                {
                    this.inputDemuxer = this.replyDemuxer = new ReplyChannelDemuxer(context);
                    flag = true;
                }
                inputDemuxer = this.replyDemuxer;
            }
            else if (!this.typeDemuxers.TryGetValue(channelType, out inputDemuxer))
            {
                inputDemuxer = this.CreateTypedDemuxer(channelType, context);
                this.typeDemuxers.Add(channelType, inputDemuxer);
                flag = true;
            }
            if (!flag)
            {
                context.RemainingBindingElements.Clear();
            }
            return inputDemuxer;
        }

        public int MaxPendingSessions
        {
            get
            {
                return this.maxPendingSessions;
            }
            set
            {
                this.maxPendingSessions = value;
            }
        }

        public TimeSpan PeekTimeout
        {
            get
            {
                return this.peekTimeout;
            }
            set
            {
                this.peekTimeout = value;
            }
        }
    }
}

