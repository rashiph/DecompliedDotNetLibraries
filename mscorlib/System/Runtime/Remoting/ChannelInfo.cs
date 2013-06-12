namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.Remoting.Channels;
    using System.Security;

    [Serializable]
    internal sealed class ChannelInfo : IChannelInfo
    {
        private object[] channelData;

        [SecurityCritical]
        internal ChannelInfo()
        {
            this.ChannelData = ChannelServices.CurrentChannelData;
        }

        public object[] ChannelData
        {
            [SecurityCritical]
            get
            {
                return this.channelData;
            }
            [SecurityCritical]
            set
            {
                this.channelData = value;
            }
        }
    }
}

