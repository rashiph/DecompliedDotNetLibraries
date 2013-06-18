namespace System.ServiceModel
{
    using System;

    internal sealed class ChannelFactoryRef<TChannel> where TChannel: class
    {
        private ChannelFactory<TChannel> channelFactory;
        private int refCount;

        public ChannelFactoryRef(ChannelFactory<TChannel> channelFactory)
        {
            this.refCount = 1;
            this.channelFactory = channelFactory;
        }

        public void Abort()
        {
            this.channelFactory.Abort();
        }

        public void AddRef()
        {
            this.refCount++;
        }

        public void Close(TimeSpan timeout)
        {
            this.channelFactory.Close(timeout);
        }

        public bool Release()
        {
            this.refCount--;
            return (this.refCount == 0);
        }

        public ChannelFactory<TChannel> ChannelFactory
        {
            get
            {
                return this.channelFactory;
            }
        }
    }
}

