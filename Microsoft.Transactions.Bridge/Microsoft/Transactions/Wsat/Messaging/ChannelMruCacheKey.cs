namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;

    internal class ChannelMruCacheKey : IEqualityComparer<ChannelMruCacheKey>
    {
        private string address;
        private EndpointIdentity identity;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ChannelMruCacheKey(string address, EndpointIdentity identity)
        {
            this.address = address;
            this.identity = identity;
        }

        public bool Equals(ChannelMruCacheKey x, ChannelMruCacheKey y)
        {
            if (!x.address.Equals(y.address))
            {
                return false;
            }
            if (x.identity == null)
            {
                return (y.identity == null);
            }
            return x.identity.Equals(y.identity);
        }

        public int GetHashCode(ChannelMruCacheKey obj)
        {
            int hashCode = obj.address.GetHashCode();
            if (obj.identity != null)
            {
                hashCode ^= obj.identity.GetHashCode();
            }
            return hashCode;
        }
    }
}

