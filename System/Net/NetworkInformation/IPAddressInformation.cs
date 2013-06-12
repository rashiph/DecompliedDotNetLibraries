namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    public abstract class IPAddressInformation
    {
        protected IPAddressInformation()
        {
        }

        public abstract IPAddress Address { get; }

        public abstract bool IsDnsEligible { get; }

        public abstract bool IsTransient { get; }
    }
}

