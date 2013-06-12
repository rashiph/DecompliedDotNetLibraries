namespace System.Net.NetworkInformation
{
    using System;

    public abstract class MulticastIPAddressInformation : IPAddressInformation
    {
        protected MulticastIPAddressInformation()
        {
        }

        public abstract long AddressPreferredLifetime { get; }

        public abstract long AddressValidLifetime { get; }

        public abstract long DhcpLeaseLifetime { get; }

        public abstract System.Net.NetworkInformation.DuplicateAddressDetectionState DuplicateAddressDetectionState { get; }

        public abstract System.Net.NetworkInformation.PrefixOrigin PrefixOrigin { get; }

        public abstract System.Net.NetworkInformation.SuffixOrigin SuffixOrigin { get; }
    }
}

