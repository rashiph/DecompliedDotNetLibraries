namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ReplyAdapterBindingElement : BindingElement
    {
        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            if (context.CanBuildInnerChannelListener<IReplySessionChannel>() || context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                return context.BuildInnerChannelListener<TChannel>();
            }
            if ((typeof(TChannel) == typeof(IReplySessionChannel)) && context.CanBuildInnerChannelListener<IDuplexSessionChannel>())
            {
                return (IChannelListener<TChannel>) new ReplySessionOverDuplexSessionChannelListener(context);
            }
            if ((typeof(TChannel) != typeof(IReplyChannel)) || !context.CanBuildInnerChannelListener<IDuplexChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", System.ServiceModel.SR.GetString("ChannelTypeNotSupported", new object[] { typeof(TChannel) }));
            }
            return (IChannelListener<TChannel>) new ReplyOverDuplexChannelListener(context);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context) where TChannel: class, IChannel
        {
            if (typeof(TChannel) == typeof(IReplySessionChannel))
            {
                if (!context.CanBuildInnerChannelListener<IReplySessionChannel>())
                {
                    return context.CanBuildInnerChannelListener<IDuplexSessionChannel>();
                }
                return true;
            }
            if (!(typeof(TChannel) == typeof(IReplyChannel)))
            {
                return false;
            }
            if (!context.CanBuildInnerChannelListener<IReplyChannel>())
            {
                return context.CanBuildInnerChannelListener<IDuplexChannel>();
            }
            return true;
        }

        public override BindingElement Clone()
        {
            return new ReplyAdapterBindingElement();
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            return context.GetInnerProperty<T>();
        }
    }
}

