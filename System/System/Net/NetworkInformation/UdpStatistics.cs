namespace System.Net.NetworkInformation
{
    using System;

    public abstract class UdpStatistics
    {
        protected UdpStatistics()
        {
        }

        public abstract long DatagramsReceived { get; }

        public abstract long DatagramsSent { get; }

        public abstract long IncomingDatagramsDiscarded { get; }

        public abstract long IncomingDatagramsWithErrors { get; }

        public abstract int UdpListeners { get; }
    }
}

