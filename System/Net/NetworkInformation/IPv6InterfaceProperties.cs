namespace System.Net.NetworkInformation
{
    using System;

    public abstract class IPv6InterfaceProperties
    {
        protected IPv6InterfaceProperties()
        {
        }

        public abstract int Index { get; }

        public abstract int Mtu { get; }
    }
}

