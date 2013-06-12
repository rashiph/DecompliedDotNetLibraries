namespace System.Net.NetworkInformation
{
    using System;

    public abstract class IPv4InterfaceProperties
    {
        protected IPv4InterfaceProperties()
        {
        }

        public abstract int Index { get; }

        public abstract bool IsAutomaticPrivateAddressingActive { get; }

        public abstract bool IsAutomaticPrivateAddressingEnabled { get; }

        public abstract bool IsDhcpEnabled { get; }

        public abstract bool IsForwardingEnabled { get; }

        public abstract int Mtu { get; }

        public abstract bool UsesWins { get; }
    }
}

