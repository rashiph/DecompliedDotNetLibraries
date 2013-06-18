namespace System.ServiceModel.Channels
{
    using System;

    internal class LateBoundChannelParameterCollection : ChannelParameterCollection
    {
        private IChannel channel;

        internal void SetChannel(IChannel channel)
        {
            this.channel = channel;
        }

        protected override IChannel Channel
        {
            get
            {
                return this.channel;
            }
        }
    }
}

