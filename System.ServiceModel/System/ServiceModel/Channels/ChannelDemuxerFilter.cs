namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Dispatcher;

    internal class ChannelDemuxerFilter
    {
        private MessageFilter filter;
        private int priority;

        public ChannelDemuxerFilter(MessageFilter filter, int priority)
        {
            this.filter = filter;
            this.priority = priority;
        }

        public MessageFilter Filter
        {
            get
            {
                return this.filter;
            }
        }

        public int Priority
        {
            get
            {
                return this.priority;
            }
        }
    }
}

