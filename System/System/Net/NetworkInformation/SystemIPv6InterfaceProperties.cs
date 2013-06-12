namespace System.Net.NetworkInformation
{
    using System;

    internal class SystemIPv6InterfaceProperties : IPv6InterfaceProperties
    {
        private uint index;
        private uint mtu;

        internal SystemIPv6InterfaceProperties(uint index, uint mtu)
        {
            this.index = index;
            this.mtu = mtu;
        }

        public override int Index
        {
            get
            {
                return (int) this.index;
            }
        }

        public override int Mtu
        {
            get
            {
                return (int) this.mtu;
            }
        }
    }
}

