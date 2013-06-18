namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal class ChannelFactoryRefCache<TChannel> : MruCache<EndpointTrait<TChannel>, ChannelFactoryRef<TChannel>> where TChannel: class
    {
        private static EndpointTraitComparer<TChannel> DefaultEndpointTraitComparer;

        static ChannelFactoryRefCache()
        {
            ChannelFactoryRefCache<TChannel>.DefaultEndpointTraitComparer = new EndpointTraitComparer<TChannel>();
        }

        public ChannelFactoryRefCache(int watermark) : base((watermark * 4) / 5, watermark, ChannelFactoryRefCache<TChannel>.DefaultEndpointTraitComparer)
        {
        }

        protected override void OnSingleItemRemoved(ChannelFactoryRef<TChannel> item)
        {
            if (item.Release())
            {
                item.Abort();
            }
        }

        private class EndpointTraitComparer : IEqualityComparer<EndpointTrait<TChannel>>
        {
            public bool Equals(EndpointTrait<TChannel> x, EndpointTrait<TChannel> y)
            {
                if (x != null)
                {
                    return ((y != null) && x.Equals(y));
                }
                if (y != null)
                {
                    return false;
                }
                return true;
            }

            public int GetHashCode(EndpointTrait<TChannel> obj)
            {
                if (obj == null)
                {
                    return 0;
                }
                return obj.GetHashCode();
            }
        }
    }
}

