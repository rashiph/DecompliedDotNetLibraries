namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    public abstract class GatewayIPAddressInformation
    {
        protected GatewayIPAddressInformation()
        {
        }

        public abstract IPAddress Address { get; }
    }
}

