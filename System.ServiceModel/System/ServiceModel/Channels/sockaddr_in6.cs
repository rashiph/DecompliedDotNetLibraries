namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct sockaddr_in6
    {
        private const int addrByteCount = 0x10;
        private const int v4MapIndex = 10;
        private const int v4Index = 12;
        private short sin6_family;
        private ushort sin6_port;
        private uint sin6_flowinfo;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x10)]
        private byte[] sin6_addr;
        private uint sin6_scope_id;
        public sockaddr_in6(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.sin6_addr = address.GetAddressBytes();
                this.sin6_scope_id = (uint) address.ScopeId;
            }
            else
            {
                byte[] addressBytes = address.GetAddressBytes();
                this.sin6_addr = new byte[0x10];
                for (int i = 0; i < 10; i++)
                {
                    this.sin6_addr[i] = 0;
                }
                this.sin6_addr[10] = 0xff;
                this.sin6_addr[11] = 0xff;
                for (int j = 12; j < 0x10; j++)
                {
                    this.sin6_addr[j] = addressBytes[j - 12];
                }
                this.sin6_scope_id = 0;
            }
            this.sin6_family = 0x17;
            this.sin6_port = 0;
            this.sin6_flowinfo = 0;
        }

        public short Family
        {
            get
            {
                return this.sin6_family;
            }
        }
        public uint FlowInfo
        {
            get
            {
                return this.sin6_flowinfo;
            }
        }
        private bool IsV4Mapped
        {
            get
            {
                if ((this.sin6_addr[10] != 0xff) || (this.sin6_addr[11] != 0xff))
                {
                    return false;
                }
                for (int i = 0; i < 10; i++)
                {
                    if (this.sin6_addr[i] != 0)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
        public ushort Port
        {
            get
            {
                return this.sin6_port;
            }
        }
        public IPAddress ToIPAddress()
        {
            if (this.sin6_family != 0x17)
            {
                throw Fx.AssertAndThrow("AddressFamily expected to be InterNetworkV6");
            }
            if (this.IsV4Mapped)
            {
                return new IPAddress(new byte[] { this.sin6_addr[12], this.sin6_addr[13], this.sin6_addr[14], this.sin6_addr[15] });
            }
            return new IPAddress(this.sin6_addr, (long) this.sin6_scope_id);
        }
    }
}

