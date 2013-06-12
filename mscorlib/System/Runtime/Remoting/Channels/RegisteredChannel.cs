namespace System.Runtime.Remoting.Channels
{
    using System;

    internal class RegisteredChannel
    {
        private IChannel channel;
        private byte flags;
        private const byte RECEIVER = 2;
        private const byte SENDER = 1;

        internal RegisteredChannel(IChannel chnl)
        {
            this.channel = chnl;
            this.flags = 0;
            if (chnl is IChannelSender)
            {
                this.flags = (byte) (this.flags | 1);
            }
            if (chnl is IChannelReceiver)
            {
                this.flags = (byte) (this.flags | 2);
            }
        }

        internal virtual bool IsReceiver()
        {
            return ((this.flags & 2) != 0);
        }

        internal virtual bool IsSender()
        {
            return ((this.flags & 1) != 0);
        }

        internal virtual IChannel Channel
        {
            get
            {
                return this.channel;
            }
        }
    }
}

