namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime.Serialization;
    using System.ServiceModel.Channels;

    [KnownType(typeof(ActionMessageFilter)), KnownType(typeof(XPathMessageFilter)), KnownType(typeof(MatchNoneMessageFilter)), DataContract, KnownType(typeof(MatchAllMessageFilter))]
    public abstract class MessageFilter
    {
        protected MessageFilter()
        {
        }

        protected internal virtual IMessageFilterTable<FilterData> CreateFilterTable<FilterData>()
        {
            return null;
        }

        public abstract bool Match(Message message);
        public abstract bool Match(MessageBuffer buffer);
    }
}

