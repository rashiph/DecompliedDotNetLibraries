namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;

    internal class SystemGatewayIPAddressInformation : GatewayIPAddressInformation
    {
        private IPAddress address;

        internal SystemGatewayIPAddressInformation(IPAddress address)
        {
            this.address = address;
        }

        public override IPAddress Address
        {
            get
            {
                return this.address;
            }
        }
    }
}

