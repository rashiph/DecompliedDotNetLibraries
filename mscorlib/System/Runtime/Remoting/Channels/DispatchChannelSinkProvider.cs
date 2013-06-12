namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Security;

    internal class DispatchChannelSinkProvider : IServerChannelSinkProvider
    {
        internal DispatchChannelSinkProvider()
        {
        }

        [SecurityCritical]
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            return new DispatchChannelSink();
        }

        [SecurityCritical]
        public void GetChannelData(IChannelDataStore channelData)
        {
        }

        public IServerChannelSinkProvider Next
        {
            [SecurityCritical]
            get
            {
                return null;
            }
            [SecurityCritical]
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}

