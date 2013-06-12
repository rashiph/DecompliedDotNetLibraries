namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    internal class SystemIPAddressInformation : IPAddressInformation
    {
        private IPAddress address;
        internal bool dnsEligible;
        internal bool transient;

        internal SystemIPAddressInformation(IPAddress address)
        {
            this.dnsEligible = true;
            this.address = address;
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                this.dnsEligible = (address.m_Address & 0xfea9L) <= 0L;
            }
        }

        internal SystemIPAddressInformation(IpAdapterAddress adapterAddress, IPAddress address)
        {
            this.dnsEligible = true;
            this.address = address;
            this.transient = (adapterAddress.flags & AdapterAddressFlags.Transient) > 0;
            this.dnsEligible = (adapterAddress.flags & AdapterAddressFlags.DnsEligible) > 0;
        }

        internal SystemIPAddressInformation(IpAdapterUnicastAddress adapterAddress, IPAddress address)
        {
            this.dnsEligible = true;
            this.address = address;
            this.transient = (adapterAddress.flags & AdapterAddressFlags.Transient) > 0;
            this.dnsEligible = (adapterAddress.flags & AdapterAddressFlags.DnsEligible) > 0;
        }

        internal static IPAddressCollection ToAddressCollection(IntPtr ptr, IPVersion versionSupported)
        {
            IPAddressCollection addresss = new IPAddressCollection();
            if (ptr != IntPtr.Zero)
            {
                IPEndPoint point;
                IpAdapterAddress address = (IpAdapterAddress) Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
                AddressFamily family = (address.address.addressLength > 0x10) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
                SocketAddress socketAddress = new SocketAddress(family, address.address.addressLength);
                Marshal.Copy(address.address.address, socketAddress.m_Buffer, 0, address.address.addressLength);
                if (family == AddressFamily.InterNetwork)
                {
                    point = (IPEndPoint) IPEndPoint.Any.Create(socketAddress);
                }
                else
                {
                    point = (IPEndPoint) IPEndPoint.IPv6Any.Create(socketAddress);
                }
                addresss.InternalAdd(point.Address);
                while (address.next != IntPtr.Zero)
                {
                    address = (IpAdapterAddress) Marshal.PtrToStructure(address.next, typeof(IpAdapterAddress));
                    family = (address.address.addressLength > 0x10) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
                    if (((family == AddressFamily.InterNetwork) && ((versionSupported & IPVersion.IPv4) > IPVersion.None)) || ((family == AddressFamily.InterNetworkV6) && ((versionSupported & IPVersion.IPv6) > IPVersion.None)))
                    {
                        socketAddress = new SocketAddress(family, address.address.addressLength);
                        Marshal.Copy(address.address.address, socketAddress.m_Buffer, 0, address.address.addressLength);
                        if (family == AddressFamily.InterNetwork)
                        {
                            point = (IPEndPoint) IPEndPoint.Any.Create(socketAddress);
                        }
                        else
                        {
                            point = (IPEndPoint) IPEndPoint.IPv6Any.Create(socketAddress);
                        }
                        addresss.InternalAdd(point.Address);
                    }
                }
            }
            return addresss;
        }

        internal static IPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr, IPVersion versionSupported)
        {
            IPAddressInformationCollection informations = new IPAddressInformationCollection();
            if (ptr != IntPtr.Zero)
            {
                IPEndPoint point;
                IpAdapterAddress adapterAddress = (IpAdapterAddress) Marshal.PtrToStructure(ptr, typeof(IpAdapterAddress));
                AddressFamily family = (adapterAddress.address.addressLength > 0x10) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
                SocketAddress socketAddress = new SocketAddress(family, adapterAddress.address.addressLength);
                Marshal.Copy(adapterAddress.address.address, socketAddress.m_Buffer, 0, adapterAddress.address.addressLength);
                if (family == AddressFamily.InterNetwork)
                {
                    point = (IPEndPoint) IPEndPoint.Any.Create(socketAddress);
                }
                else
                {
                    point = (IPEndPoint) IPEndPoint.IPv6Any.Create(socketAddress);
                }
                informations.InternalAdd(new SystemIPAddressInformation(adapterAddress, point.Address));
                while (adapterAddress.next != IntPtr.Zero)
                {
                    adapterAddress = (IpAdapterAddress) Marshal.PtrToStructure(adapterAddress.next, typeof(IpAdapterAddress));
                    family = (adapterAddress.address.addressLength > 0x10) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
                    if (((family == AddressFamily.InterNetwork) && ((versionSupported & IPVersion.IPv4) > IPVersion.None)) || ((family == AddressFamily.InterNetworkV6) && ((versionSupported & IPVersion.IPv6) > IPVersion.None)))
                    {
                        socketAddress = new SocketAddress(family, adapterAddress.address.addressLength);
                        Marshal.Copy(adapterAddress.address.address, socketAddress.m_Buffer, 0, adapterAddress.address.addressLength);
                        if (family == AddressFamily.InterNetwork)
                        {
                            point = (IPEndPoint) IPEndPoint.Any.Create(socketAddress);
                        }
                        else
                        {
                            point = (IPEndPoint) IPEndPoint.IPv6Any.Create(socketAddress);
                        }
                        informations.InternalAdd(new SystemIPAddressInformation(adapterAddress, point.Address));
                    }
                }
            }
            return informations;
        }

        public override IPAddress Address
        {
            get
            {
                return this.address;
            }
        }

        public override bool IsDnsEligible
        {
            get
            {
                return this.dnsEligible;
            }
        }

        public override bool IsTransient
        {
            get
            {
                return this.transient;
            }
        }
    }
}

