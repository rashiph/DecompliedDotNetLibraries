namespace System.Net
{
    using System;
    using System.Globalization;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable]
    public class IPAddress
    {
        public static readonly IPAddress Any = new IPAddress(0);
        public static readonly IPAddress Broadcast = new IPAddress(0xffffffffL);
        internal const string InaddrNoneString = "255.255.255.255";
        internal const string InaddrNoneStringHex = "0xff.0xff.0xff.0xff";
        internal const string InaddrNoneStringOct = "0377.0377.0377.0377";
        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 0x10;
        public static readonly IPAddress IPv6Any;
        public static readonly IPAddress IPv6Loopback;
        public static readonly IPAddress IPv6None;
        public static readonly IPAddress Loopback = new IPAddress(0x100007f);
        internal const long LoopbackMask = 0xffL;
        internal long m_Address;
        private System.Net.Sockets.AddressFamily m_Family;
        private int m_HashCode;
        private ushort[] m_Numbers;
        private long m_ScopeId;
        [NonSerialized]
        internal string m_ToString;
        public static readonly IPAddress None = Broadcast;
        internal const int NumberOfLabels = 8;

        static IPAddress()
        {
            byte[] address = new byte[0x10];
            IPv6Any = new IPAddress(address, 0L);
            byte[] buffer2 = new byte[0x10];
            buffer2[15] = 1;
            IPv6Loopback = new IPAddress(buffer2, 0L);
            byte[] buffer3 = new byte[0x10];
            IPv6None = new IPAddress(buffer3, 0L);
        }

        internal IPAddress(int newAddress)
        {
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            this.m_Address = newAddress & ((long) 0xffffffffL);
        }

        public IPAddress(long newAddress)
        {
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if ((newAddress < 0L) || (newAddress > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("newAddress");
            }
            this.m_Address = newAddress;
        }

        public IPAddress(byte[] address)
        {
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if ((address.Length != 4) && (address.Length != 0x10))
            {
                throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "address");
            }
            if (address.Length == 4)
            {
                this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
                this.m_Address = ((((address[3] << 0x18) | (address[2] << 0x10)) | (address[1] << 8)) | address[0]) & ((long) 0xffffffffL);
            }
            else
            {
                this.m_Family = System.Net.Sockets.AddressFamily.InterNetworkV6;
                for (int i = 0; i < 8; i++)
                {
                    this.m_Numbers[i] = (ushort) ((address[i * 2] * 0x100) + address[(i * 2) + 1]);
                }
            }
        }

        public IPAddress(byte[] address, long scopeid)
        {
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length != 0x10)
            {
                throw new ArgumentException(SR.GetString("dns_bad_ip_address"), "address");
            }
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetworkV6;
            for (int i = 0; i < 8; i++)
            {
                this.m_Numbers[i] = (ushort) ((address[i * 2] * 0x100) + address[(i * 2) + 1]);
            }
            if ((scopeid < 0L) || (scopeid > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("scopeid");
            }
            this.m_ScopeId = scopeid;
        }

        private IPAddress(ushort[] address, uint scopeid)
        {
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            this.m_Family = System.Net.Sockets.AddressFamily.InterNetworkV6;
            this.m_Numbers = address;
            this.m_ScopeId = scopeid;
        }

        public override bool Equals(object comparand)
        {
            return this.Equals(comparand, true);
        }

        internal bool Equals(object comparand, bool compareScopeId)
        {
            if (!(comparand is IPAddress))
            {
                return false;
            }
            if (this.m_Family != ((IPAddress) comparand).m_Family)
            {
                return false;
            }
            if (this.m_Family != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return (((IPAddress) comparand).m_Address == this.m_Address);
            }
            for (int i = 0; i < 8; i++)
            {
                if (((IPAddress) comparand).m_Numbers[i] != this.m_Numbers[i])
                {
                    return false;
                }
            }
            return ((((IPAddress) comparand).m_ScopeId == this.m_ScopeId) || !compareScopeId);
        }

        public byte[] GetAddressBytes()
        {
            if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                byte[] buffer = new byte[0x10];
                int num = 0;
                for (int i = 0; i < 8; i++)
                {
                    buffer[num++] = (byte) ((this.m_Numbers[i] >> 8) & 0xff);
                    buffer[num++] = (byte) (this.m_Numbers[i] & 0xff);
                }
                return buffer;
            }
            return new byte[] { ((byte) this.m_Address), ((byte) (this.m_Address >> 8)), ((byte) (this.m_Address >> 0x10)), ((byte) (this.m_Address >> 0x18)) };
        }

        public override int GetHashCode()
        {
            if (this.m_Family != System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return (int) this.m_Address;
            }
            if (this.m_HashCode == 0)
            {
                this.m_HashCode = Uri.CalculateCaseInsensitiveHashCode(this.ToString());
            }
            return this.m_HashCode;
        }

        public static short HostToNetworkOrder(short host)
        {
            return (short) (((host & 0xff) << 8) | ((host >> 8) & 0xff));
        }

        public static int HostToNetworkOrder(int host)
        {
            return (((HostToNetworkOrder((short) host) & 0xffff) << 0x10) | (HostToNetworkOrder((short) (host >> 0x10)) & 0xffff));
        }

        public static long HostToNetworkOrder(long host)
        {
            return (long) (((HostToNetworkOrder((int) host) & 0xffffffffL) << 0x20) | (HostToNetworkOrder((int) (host >> 0x20)) & 0xffffffffL));
        }

        private static unsafe IPAddress InternalParse(string ipString, bool tryParse)
        {
            if (ipString == null)
            {
                throw new ArgumentNullException("ipString");
            }
            if (ipString.IndexOf(':') != -1)
            {
                SocketException innerException = null;
                if (Socket.OSSupportsIPv6)
                {
                    byte[] buffer = new byte[0x10];
                    SocketAddress address = new SocketAddress(System.Net.Sockets.AddressFamily.InterNetworkV6, 0x1c);
                    if (UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(ipString, System.Net.Sockets.AddressFamily.InterNetworkV6, IntPtr.Zero, address.m_Buffer, ref address.m_Size) == SocketError.Success)
                    {
                        for (int i = 0; i < 0x10; i++)
                        {
                            buffer[i] = address[i + 8];
                        }
                        return new IPAddress(buffer, (((address[0x1b] << 0x18) + (address[0x1a] << 0x10)) + (address[0x19] << 8)) + address[0x18]);
                    }
                    if (tryParse)
                    {
                        return null;
                    }
                    innerException = new SocketException();
                }
                else
                {
                    int start = 0;
                    if (ipString[0] != '[')
                    {
                        ipString = ipString + ']';
                    }
                    else
                    {
                        start = 1;
                    }
                    int length = ipString.Length;
                    fixed (char* str2 = ((char*) ipString))
                    {
                        char* name = str2;
                        if (IPv6AddressHelper.IsValidStrict(name, start, ref length) || (length != ipString.Length))
                        {
                            uint num5;
                            ushort[] numArray = new ushort[8];
                            string scopeId = null;
                            fixed (ushort* numRef = numArray)
                            {
                                IPv6AddressHelper.Parse(ipString, numRef, 0, ref scopeId);
                            }
                            if ((scopeId == null) || (scopeId.Length == 0))
                            {
                                return new IPAddress(numArray, 0);
                            }
                            if (uint.TryParse(scopeId.Substring(1), NumberStyles.None, null, out num5))
                            {
                                return new IPAddress(numArray, num5);
                            }
                        }
                    }
                    if (tryParse)
                    {
                        return null;
                    }
                    innerException = new SocketException(SocketError.InvalidArgument);
                }
                throw new FormatException(SR.GetString("dns_bad_ip_address"), innerException);
            }
            int newAddress = -1;
            if ((((ipString.Length > 0) && (ipString[0] >= '0')) && (ipString[0] <= '9')) && ((((ipString[ipString.Length - 1] >= '0') && (ipString[ipString.Length - 1] <= '9')) || ((ipString[ipString.Length - 1] >= 'a') && (ipString[ipString.Length - 1] <= 'f'))) || ((ipString[ipString.Length - 1] >= 'A') && (ipString[ipString.Length - 1] <= 'F'))))
            {
                Socket.InitializeSockets();
                newAddress = UnsafeNclNativeMethods.OSSOCK.inet_addr(ipString);
            }
            if (((newAddress == -1) && (string.Compare(ipString, "255.255.255.255", StringComparison.Ordinal) != 0)) && ((string.Compare(ipString, "0xff.0xff.0xff.0xff", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(ipString, "0377.0377.0377.0377", StringComparison.Ordinal) != 0)))
            {
                if (!tryParse)
                {
                    throw new FormatException(SR.GetString("dns_bad_ip_address"));
                }
                return null;
            }
            return new IPAddress(newAddress);
        }

        public static bool IsLoopback(IPAddress address)
        {
            if (address.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                return address.Equals(IPv6Loopback);
            }
            return ((address.m_Address & 0xffL) == (Loopback.m_Address & 0xffL));
        }

        public static short NetworkToHostOrder(short network)
        {
            return HostToNetworkOrder(network);
        }

        public static int NetworkToHostOrder(int network)
        {
            return HostToNetworkOrder(network);
        }

        public static long NetworkToHostOrder(long network)
        {
            return HostToNetworkOrder(network);
        }

        public static IPAddress Parse(string ipString)
        {
            return InternalParse(ipString, false);
        }

        internal IPAddress Snapshot()
        {
            System.Net.Sockets.AddressFamily family = this.m_Family;
            if (family != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (family != System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new InternalException();
                }
                return new IPAddress(this.m_Numbers, (uint) this.m_ScopeId);
            }
            return new IPAddress(this.m_Address);
        }

        public override unsafe string ToString()
        {
            if (this.m_ToString == null)
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    int capacity = 0x100;
                    StringBuilder addressString = new StringBuilder(capacity);
                    if (Socket.OSSupportsIPv6)
                    {
                        SocketAddress address = new SocketAddress(System.Net.Sockets.AddressFamily.InterNetworkV6, 0x1c);
                        int num2 = 8;
                        for (int i = 0; i < 8; i++)
                        {
                            address[num2++] = (byte) (this.m_Numbers[i] >> 8);
                            address[num2++] = (byte) this.m_Numbers[i];
                        }
                        if (this.m_ScopeId > 0L)
                        {
                            address[0x18] = (byte) this.m_ScopeId;
                            address[0x19] = (byte) (this.m_ScopeId >> 8);
                            address[0x1a] = (byte) (this.m_ScopeId >> 0x10);
                            address[0x1b] = (byte) (this.m_ScopeId >> 0x18);
                        }
                        if (UnsafeNclNativeMethods.OSSOCK.WSAAddressToString(address.m_Buffer, address.m_Size, IntPtr.Zero, addressString, ref capacity) != SocketError.Success)
                        {
                            throw new SocketException();
                        }
                    }
                    else
                    {
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[0] })).Append(':');
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[1] })).Append(':');
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[2] })).Append(':');
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[3] })).Append(':');
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[4] })).Append(':');
                        addressString.Append(string.Format(CultureInfo.InvariantCulture, "{0:x4}", new object[] { this.m_Numbers[5] })).Append(':');
                        addressString.Append((int) ((this.m_Numbers[6] >> 8) & 0xff)).Append('.');
                        addressString.Append((int) (this.m_Numbers[6] & 0xff)).Append('.');
                        addressString.Append((int) ((this.m_Numbers[7] >> 8) & 0xff)).Append('.');
                        addressString.Append((int) (this.m_Numbers[7] & 0xff));
                        if (this.m_ScopeId != 0L)
                        {
                            addressString.Append('%').Append((uint) this.m_ScopeId);
                        }
                    }
                    this.m_ToString = addressString.ToString();
                }
                else
                {
                    int startIndex = 15;
                    char* chPtr = (char*) stackalloc byte[(((IntPtr) 15) * 2)];
                    int num5 = (int) ((this.m_Address >> 0x18) & 0xffL);
                    do
                    {
                        chPtr[--startIndex] = (char) (0x30 + (num5 % 10));
                        num5 /= 10;
                    }
                    while (num5 > 0);
                    chPtr[--startIndex] = '.';
                    num5 = (int) ((this.m_Address >> 0x10) & 0xffL);
                    do
                    {
                        chPtr[--startIndex] = (char) (0x30 + (num5 % 10));
                        num5 /= 10;
                    }
                    while (num5 > 0);
                    chPtr[--startIndex] = '.';
                    num5 = (int) ((this.m_Address >> 8) & 0xffL);
                    do
                    {
                        chPtr[--startIndex] = (char) (0x30 + (num5 % 10));
                        num5 /= 10;
                    }
                    while (num5 > 0);
                    chPtr[--startIndex] = '.';
                    num5 = (int) (this.m_Address & 0xffL);
                    do
                    {
                        chPtr[--startIndex] = (char) (0x30 + (num5 % 10));
                        num5 /= 10;
                    }
                    while (num5 > 0);
                    this.m_ToString = new string(chPtr, startIndex, 15 - startIndex);
                }
            }
            return this.m_ToString;
        }

        public static bool TryParse(string ipString, out IPAddress address)
        {
            address = InternalParse(ipString, true);
            return (address != null);
        }

        [Obsolete("This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons. http://go.microsoft.com/fwlink/?linkid=14202")]
        public long Address
        {
            get
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                return this.m_Address;
            }
            set
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                if (this.m_Address != value)
                {
                    this.m_ToString = null;
                    this.m_Address = value;
                }
            }
        }

        public System.Net.Sockets.AddressFamily AddressFamily
        {
            get
            {
                return this.m_Family;
            }
        }

        internal bool IsBroadcast
        {
            get
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    return false;
                }
                return (this.m_Address == Broadcast.m_Address);
            }
        }

        public bool IsIPv6LinkLocal
        {
            get
            {
                return ((this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xffc0) == 0xfe80));
            }
        }

        public bool IsIPv6Multicast
        {
            get
            {
                return ((this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xff00) == 0xff00));
            }
        }

        public bool IsIPv6SiteLocal
        {
            get
            {
                return ((this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xffc0) == 0xfec0));
            }
        }

        public bool IsIPv6Teredo
        {
            get
            {
                return (((this.m_Family == System.Net.Sockets.AddressFamily.InterNetworkV6) && (this.m_Numbers[0] == 0x2001)) && (this.m_Numbers[1] == 0));
            }
        }

        public long ScopeId
        {
            get
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                return this.m_ScopeId;
            }
            set
            {
                if (this.m_Family == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    throw new SocketException(SocketError.OperationNotSupported);
                }
                if ((value < 0L) || (value > 0xffffffffL))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.m_ScopeId != value)
                {
                    this.m_Address = value;
                    this.m_ScopeId = value;
                }
            }
        }
    }
}

