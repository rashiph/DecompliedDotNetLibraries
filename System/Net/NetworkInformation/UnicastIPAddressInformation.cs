namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    public abstract class UnicastIPAddressInformation : IPAddressInformation
    {
        protected UnicastIPAddressInformation()
        {
        }

        public abstract long AddressPreferredLifetime { get; }

        public abstract long AddressValidLifetime { get; }

        public abstract long DhcpLeaseLifetime { get; }

        public abstract System.Net.NetworkInformation.DuplicateAddressDetectionState DuplicateAddressDetectionState { get; }

        public abstract IPAddress IPv4Mask { get; }

        public abstract System.Net.NetworkInformation.PrefixOrigin PrefixOrigin { get; }

        public abstract System.Net.NetworkInformation.SuffixOrigin SuffixOrigin { get; }
    }
}

