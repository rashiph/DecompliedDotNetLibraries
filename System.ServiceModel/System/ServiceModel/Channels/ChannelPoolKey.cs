namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal class ChannelPoolKey : IEquatable<ChannelPoolKey>
    {
        private EndpointAddress address;
        private Uri via;

        public ChannelPoolKey(EndpointAddress address, Uri via)
        {
            this.address = address;
            this.via = via;
        }

        public bool Equals(ChannelPoolKey other)
        {
            return (this.address.EndpointEquals(other.address) && this.via.Equals(other.via));
        }

        public override int GetHashCode()
        {
            return (this.address.GetHashCode() + this.via.GetHashCode());
        }
    }
}

