namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Security.Permissions;

    public class UdpClient : IDisposable
    {
        private bool m_Active;
        private byte[] m_Buffer;
        private bool m_CleanedUp;
        private Socket m_ClientSocket;
        private AddressFamily m_Family;
        private bool m_IsBroadcast;
        private const int MaxUDPSize = 0x10000;

        public UdpClient() : this(AddressFamily.InterNetwork)
        {
        }

        public UdpClient(int port) : this(port, AddressFamily.InterNetwork)
        {
        }

        public UdpClient(IPEndPoint localEP)
        {
            this.m_Buffer = new byte[0x10000];
            this.m_Family = AddressFamily.InterNetwork;
            if (localEP == null)
            {
                throw new ArgumentNullException("localEP");
            }
            this.m_Family = localEP.AddressFamily;
            this.createClientSocket();
            this.Client.Bind(localEP);
        }

        public UdpClient(AddressFamily family)
        {
            this.m_Buffer = new byte[0x10000];
            this.m_Family = AddressFamily.InterNetwork;
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException(SR.GetString("net_protocol_invalid_family", new object[] { "UDP" }), "family");
            }
            this.m_Family = family;
            this.createClientSocket();
        }

        public UdpClient(int port, AddressFamily family)
        {
            IPEndPoint point;
            this.m_Buffer = new byte[0x10000];
            this.m_Family = AddressFamily.InterNetwork;
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            if ((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6))
            {
                throw new ArgumentException(SR.GetString("net_protocol_invalid_family"), "family");
            }
            this.m_Family = family;
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                point = new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                point = new IPEndPoint(IPAddress.IPv6Any, port);
            }
            this.createClientSocket();
            this.Client.Bind(point);
        }

        public UdpClient(string hostname, int port)
        {
            this.m_Buffer = new byte[0x10000];
            this.m_Family = AddressFamily.InterNetwork;
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.Connect(hostname, port);
        }

        public void AllowNatTraversal(bool allowed)
        {
            if (allowed)
            {
                this.m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.Unrestricted);
            }
            else
            {
                this.m_ClientSocket.SetIPProtectionLevel(IPProtectionLevel.EdgeRestricted);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginReceive(AsyncCallback requestCallback, object state)
        {
            EndPoint any;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                any = IPEndPoint.Any;
            }
            else
            {
                any = IPEndPoint.IPv6Any;
            }
            return this.Client.BeginReceiveFrom(this.m_Buffer, 0, 0x10000, SocketFlags.None, ref any, requestCallback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, AsyncCallback requestCallback, object state)
        {
            return this.BeginSend(datagram, bytes, null, requestCallback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, IPEndPoint endPoint, AsyncCallback requestCallback, object state)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (datagram == null)
            {
                throw new ArgumentNullException("datagram");
            }
            if (bytes > datagram.Length)
            {
                throw new ArgumentOutOfRangeException("bytes");
            }
            if (this.m_Active && (endPoint != null))
            {
                throw new InvalidOperationException(SR.GetString("net_udpconnected"));
            }
            if (endPoint == null)
            {
                return this.Client.BeginSend(datagram, 0, bytes, SocketFlags.None, requestCallback, state);
            }
            this.CheckForBroadcast(endPoint.Address);
            return this.Client.BeginSendTo(datagram, 0, bytes, SocketFlags.None, endPoint, requestCallback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public IAsyncResult BeginSend(byte[] datagram, int bytes, string hostname, int port, AsyncCallback requestCallback, object state)
        {
            if (this.m_Active && ((hostname != null) || (port != 0)))
            {
                throw new InvalidOperationException(SR.GetString("net_udpconnected"));
            }
            IPEndPoint endPoint = null;
            if ((hostname != null) && (port != 0))
            {
                IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
                int index = 0;
                while ((index < hostAddresses.Length) && (hostAddresses[index].AddressFamily != this.m_Family))
                {
                    index++;
                }
                if ((hostAddresses.Length == 0) || (index == hostAddresses.Length))
                {
                    throw new ArgumentException(SR.GetString("net_invalidAddressList"), "hostname");
                }
                this.CheckForBroadcast(hostAddresses[index]);
                endPoint = new IPEndPoint(hostAddresses[index], port);
            }
            return this.BeginSend(datagram, bytes, endPoint, requestCallback, state);
        }

        private void CheckForBroadcast(IPAddress ipAddress)
        {
            if (((this.Client != null) && !this.m_IsBroadcast) && ipAddress.IsBroadcast)
            {
                this.m_IsBroadcast = true;
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
        }

        public void Close()
        {
            this.Dispose(true);
        }

        public void Connect(IPEndPoint endPoint)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }
            this.CheckForBroadcast(endPoint.Address);
            this.Client.Connect(endPoint);
            this.m_Active = true;
        }

        public void Connect(IPAddress addr, int port)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (addr == null)
            {
                throw new ArgumentNullException("addr");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            IPEndPoint endPoint = new IPEndPoint(addr, port);
            this.Connect(endPoint);
        }

        public void Connect(string hostname, int port)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (hostname == null)
            {
                throw new ArgumentNullException("hostname");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            Exception exception = null;
            Socket socket = null;
            Socket socket2 = null;
            try
            {
                if (this.m_ClientSocket == null)
                {
                    if (Socket.OSSupportsIPv4)
                    {
                        socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    }
                    if (Socket.OSSupportsIPv6)
                    {
                        socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
                    }
                }
                foreach (IPAddress address in hostAddresses)
                {
                    try
                    {
                        if (this.m_ClientSocket == null)
                        {
                            if ((address.AddressFamily == AddressFamily.InterNetwork) && (socket2 != null))
                            {
                                socket2.Connect(address, port);
                                this.m_ClientSocket = socket2;
                                if (socket != null)
                                {
                                    socket.Close();
                                }
                            }
                            else if (socket != null)
                            {
                                socket.Connect(address, port);
                                this.m_ClientSocket = socket;
                                if (socket2 != null)
                                {
                                    socket2.Close();
                                }
                            }
                            this.m_Family = address.AddressFamily;
                            this.m_Active = true;
                            return;
                        }
                        if (address.AddressFamily == this.m_Family)
                        {
                            this.Connect(new IPEndPoint(address, port));
                            this.m_Active = true;
                            return;
                        }
                    }
                    catch (Exception exception2)
                    {
                        if (NclUtilities.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                }
            }
            catch (Exception exception3)
            {
                if (NclUtilities.IsFatal(exception3))
                {
                    throw;
                }
                exception = exception3;
            }
            finally
            {
                if (!this.m_Active)
                {
                    if (socket != null)
                    {
                        socket.Close();
                    }
                    if (socket2 != null)
                    {
                        socket2.Close();
                    }
                    if (exception != null)
                    {
                        throw exception;
                    }
                    throw new SocketException(SocketError.NotConnected);
                }
            }
        }

        private void createClientSocket()
        {
            this.Client = new Socket(this.m_Family, SocketType.Dgram, ProtocolType.Udp);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.FreeResources();
                GC.SuppressFinalize(this);
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != this.m_Family)
            {
                throw new ArgumentException(SR.GetString("net_protocol_invalid_multicast_family", new object[] { "UDP" }), "multicastAddr");
            }
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption option2 = new IPv6MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, option2);
            }
        }

        public void DropMulticastGroup(IPAddress multicastAddr, int ifindex)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException(SR.GetString("net_value_cannot_be_negative"), "ifindex");
            }
            if (this.m_Family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException(SocketError.OperationNotSupported);
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, (long) ifindex);
            this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.DropMembership, optionValue);
        }

        public byte[] EndReceive(IAsyncResult asyncResult, ref IPEndPoint remoteEP)
        {
            EndPoint any;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                any = IPEndPoint.Any;
            }
            else
            {
                any = IPEndPoint.IPv6Any;
            }
            int count = this.Client.EndReceiveFrom(asyncResult, ref any);
            remoteEP = (IPEndPoint) any;
            if (count < 0x10000)
            {
                byte[] dst = new byte[count];
                Buffer.BlockCopy(this.m_Buffer, 0, dst, 0, count);
                return dst;
            }
            return this.m_Buffer;
        }

        public int EndSend(IAsyncResult asyncResult)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_Active)
            {
                return this.Client.EndSend(asyncResult);
            }
            return this.Client.EndSendTo(asyncResult);
        }

        private void FreeResources()
        {
            if (!this.m_CleanedUp)
            {
                Socket client = this.Client;
                if (client != null)
                {
                    client.InternalShutdown(SocketShutdown.Both);
                    client.Close();
                    this.Client = null;
                }
                this.m_CleanedUp = true;
            }
        }

        public void JoinMulticastGroup(IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (multicastAddr.AddressFamily != this.m_Family)
            {
                throw new ArgumentException(SR.GetString("net_protocol_invalid_multicast_family", new object[] { "UDP" }), "multicastAddr");
            }
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                MulticastOption optionValue = new MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
            }
            else
            {
                IPv6MulticastOption option2 = new IPv6MulticastOption(multicastAddr);
                this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, option2);
            }
        }

        public void JoinMulticastGroup(int ifindex, IPAddress multicastAddr)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (ifindex < 0)
            {
                throw new ArgumentException(SR.GetString("net_value_cannot_be_negative"), "ifindex");
            }
            if (this.m_Family != AddressFamily.InterNetworkV6)
            {
                throw new SocketException(SocketError.OperationNotSupported);
            }
            IPv6MulticastOption optionValue = new IPv6MulticastOption(multicastAddr, (long) ifindex);
            this.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, optionValue);
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, int timeToLive)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (multicastAddr == null)
            {
                throw new ArgumentNullException("multicastAddr");
            }
            if (!ValidationHelper.ValidateRange(timeToLive, 0, 0xff))
            {
                throw new ArgumentOutOfRangeException("timeToLive");
            }
            this.JoinMulticastGroup(multicastAddr);
            this.Client.SetSocketOption((this.m_Family == AddressFamily.InterNetwork) ? SocketOptionLevel.IP : SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, timeToLive);
        }

        public void JoinMulticastGroup(IPAddress multicastAddr, IPAddress localAddress)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_Family != AddressFamily.InterNetwork)
            {
                throw new SocketException(SocketError.OperationNotSupported);
            }
            MulticastOption optionValue = new MulticastOption(multicastAddr, localAddress);
            this.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, optionValue);
        }

        public byte[] Receive(ref IPEndPoint remoteEP)
        {
            EndPoint any;
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (this.m_Family == AddressFamily.InterNetwork)
            {
                any = IPEndPoint.Any;
            }
            else
            {
                any = IPEndPoint.IPv6Any;
            }
            int count = this.Client.ReceiveFrom(this.m_Buffer, 0x10000, SocketFlags.None, ref any);
            remoteEP = (IPEndPoint) any;
            if (count < 0x10000)
            {
                byte[] dst = new byte[count];
                Buffer.BlockCopy(this.m_Buffer, 0, dst, 0, count);
                return dst;
            }
            return this.m_Buffer;
        }

        public int Send(byte[] dgram, int bytes)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (!this.m_Active)
            {
                throw new InvalidOperationException(SR.GetString("net_notconnected"));
            }
            return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
        }

        public int Send(byte[] dgram, int bytes, IPEndPoint endPoint)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (this.m_Active && (endPoint != null))
            {
                throw new InvalidOperationException(SR.GetString("net_udpconnected"));
            }
            if (endPoint == null)
            {
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
            }
            this.CheckForBroadcast(endPoint.Address);
            return this.Client.SendTo(dgram, 0, bytes, SocketFlags.None, endPoint);
        }

        public int Send(byte[] dgram, int bytes, string hostname, int port)
        {
            if (this.m_CleanedUp)
            {
                throw new ObjectDisposedException(base.GetType().FullName);
            }
            if (dgram == null)
            {
                throw new ArgumentNullException("dgram");
            }
            if (this.m_Active && ((hostname != null) || (port != 0)))
            {
                throw new InvalidOperationException(SR.GetString("net_udpconnected"));
            }
            if ((hostname == null) || (port == 0))
            {
                return this.Client.Send(dgram, 0, bytes, SocketFlags.None);
            }
            IPAddress[] hostAddresses = Dns.GetHostAddresses(hostname);
            int index = 0;
            while ((index < hostAddresses.Length) && (hostAddresses[index].AddressFamily != this.m_Family))
            {
                index++;
            }
            if ((hostAddresses.Length == 0) || (index == hostAddresses.Length))
            {
                throw new ArgumentException(SR.GetString("net_invalidAddressList"), "hostname");
            }
            this.CheckForBroadcast(hostAddresses[index]);
            IPEndPoint remoteEP = new IPEndPoint(hostAddresses[index], port);
            return this.Client.SendTo(dgram, 0, bytes, SocketFlags.None, remoteEP);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        protected bool Active
        {
            get
            {
                return this.m_Active;
            }
            set
            {
                this.m_Active = value;
            }
        }

        public int Available
        {
            get
            {
                return this.m_ClientSocket.Available;
            }
        }

        public Socket Client
        {
            get
            {
                return this.m_ClientSocket;
            }
            set
            {
                this.m_ClientSocket = value;
            }
        }

        public bool DontFragment
        {
            get
            {
                return this.m_ClientSocket.DontFragment;
            }
            set
            {
                this.m_ClientSocket.DontFragment = value;
            }
        }

        public bool EnableBroadcast
        {
            get
            {
                return this.m_ClientSocket.EnableBroadcast;
            }
            set
            {
                this.m_ClientSocket.EnableBroadcast = value;
            }
        }

        public bool ExclusiveAddressUse
        {
            get
            {
                return this.m_ClientSocket.ExclusiveAddressUse;
            }
            set
            {
                this.m_ClientSocket.ExclusiveAddressUse = value;
            }
        }

        public bool MulticastLoopback
        {
            get
            {
                return this.m_ClientSocket.MulticastLoopback;
            }
            set
            {
                this.m_ClientSocket.MulticastLoopback = value;
            }
        }

        public short Ttl
        {
            get
            {
                return this.m_ClientSocket.Ttl;
            }
            set
            {
                this.m_ClientSocket.Ttl = value;
            }
        }
    }
}

