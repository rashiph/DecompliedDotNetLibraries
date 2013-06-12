namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;

    [Serializable]
    public class IPEndPoint : EndPoint
    {
        internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);
        internal const int AnyPort = 0;
        internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);
        private IPAddress m_Address;
        private int m_Port;
        public const int MaxPort = 0xffff;
        public const int MinPort = 0;

        public IPEndPoint(long address, int port)
        {
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_Port = port;
            this.m_Address = new IPAddress(address);
        }

        public IPEndPoint(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_Port = port;
            this.m_Address = address;
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != this.AddressFamily)
            {
                throw new ArgumentException(SR.GetString("net_InvalidAddressFamily", new object[] { socketAddress.Family.ToString(), base.GetType().FullName, this.AddressFamily.ToString() }), "socketAddress");
            }
            if (socketAddress.Size < 8)
            {
                throw new ArgumentException(SR.GetString("net_InvalidSocketAddressSize", new object[] { socketAddress.GetType().FullName, base.GetType().FullName }), "socketAddress");
            }
            if (this.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                byte[] address = new byte[0x10];
                for (int i = 0; i < address.Length; i++)
                {
                    address[i] = socketAddress[i + 8];
                }
                int num2 = ((socketAddress[2] << 8) & 0xff00) | socketAddress[3];
                long scopeid = (((socketAddress[0x1b] << 0x18) + (socketAddress[0x1a] << 0x10)) + (socketAddress[0x19] << 8)) + socketAddress[0x18];
                return new IPEndPoint(new IPAddress(address, scopeid), num2);
            }
            int port = ((socketAddress[2] << 8) & 0xff00) | socketAddress[3];
            return new IPEndPoint(((((socketAddress[4] & 0xff) | ((socketAddress[5] << 8) & 0xff00)) | ((socketAddress[6] << 0x10) & 0xff0000)) | (socketAddress[7] << 0x18)) & ((long) 0xffffffffL), port);
        }

        public override bool Equals(object comparand)
        {
            return ((comparand is IPEndPoint) && (((IPEndPoint) comparand).m_Address.Equals(this.m_Address) && (((IPEndPoint) comparand).m_Port == this.m_Port)));
        }

        public override int GetHashCode()
        {
            return (this.m_Address.GetHashCode() ^ this.m_Port);
        }

        public override SocketAddress Serialize()
        {
            if (this.m_Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                SocketAddress address = new SocketAddress(this.AddressFamily, 0x1c);
                int port = this.Port;
                address[2] = (byte) (port >> 8);
                address[3] = (byte) port;
                address[4] = 0;
                address[5] = 0;
                address[6] = 0;
                address[7] = 0;
                long scopeId = this.Address.ScopeId;
                address[0x18] = (byte) scopeId;
                address[0x19] = (byte) (scopeId >> 8);
                address[0x1a] = (byte) (scopeId >> 0x10);
                address[0x1b] = (byte) (scopeId >> 0x18);
                byte[] addressBytes = this.Address.GetAddressBytes();
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    address[8 + i] = addressBytes[i];
                }
                return address;
            }
            SocketAddress address2 = new SocketAddress(this.m_Address.AddressFamily, 0x10);
            address2[2] = (byte) (this.Port >> 8);
            address2[3] = (byte) this.Port;
            address2[4] = (byte) this.Address.m_Address;
            address2[5] = (byte) (this.Address.m_Address >> 8);
            address2[6] = (byte) (this.Address.m_Address >> 0x10);
            address2[7] = (byte) (this.Address.m_Address >> 0x18);
            return address2;
        }

        internal IPEndPoint Snapshot()
        {
            return new IPEndPoint(this.Address.Snapshot(), this.Port);
        }

        public override string ToString()
        {
            string str;
            if (this.m_Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                str = "[{0}]:{1}";
            }
            else
            {
                str = "{0}:{1}";
            }
            return string.Format(str, this.m_Address.ToString(), this.Port.ToString(NumberFormatInfo.InvariantInfo));
        }

        public IPAddress Address
        {
            get
            {
                return this.m_Address;
            }
            set
            {
                this.m_Address = value;
            }
        }

        public override System.Net.Sockets.AddressFamily AddressFamily
        {
            get
            {
                return this.m_Address.AddressFamily;
            }
        }

        public int Port
        {
            get
            {
                return this.m_Port;
            }
            set
            {
                if (!ValidationHelper.ValidateTcpPort(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_Port = value;
            }
        }
    }
}

