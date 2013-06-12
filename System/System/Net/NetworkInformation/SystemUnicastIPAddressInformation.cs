namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    internal class SystemUnicastIPAddressInformation : UnicastIPAddressInformation
    {
        private IpAdapterUnicastAddress adapterAddress;
        private long dhcpLeaseLifetime;
        private SystemIPAddressInformation innerInfo;
        internal IPAddress ipv4Mask;

        private SystemUnicastIPAddressInformation()
        {
        }

        internal SystemUnicastIPAddressInformation(IpAdapterInfo ipAdapterInfo, IPExtendedAddress address)
        {
            this.innerInfo = new SystemIPAddressInformation(address.address);
            DateTime time = new DateTime(0x7b2, 1, 1);
            time = time.AddSeconds((double) ipAdapterInfo.leaseExpires);
            TimeSpan span = (TimeSpan) (time - DateTime.UtcNow);
            this.dhcpLeaseLifetime = (long) span.TotalSeconds;
            this.ipv4Mask = address.mask;
        }

        internal SystemUnicastIPAddressInformation(IpAdapterUnicastAddress adapterAddress, IPAddress ipAddress)
        {
            this.innerInfo = new SystemIPAddressInformation(adapterAddress, ipAddress);
            this.adapterAddress = adapterAddress;
            this.dhcpLeaseLifetime = adapterAddress.leaseLifetime;
        }

        internal static UnicastIPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr)
        {
            UnicastIPAddressInformationCollection informations = new UnicastIPAddressInformationCollection();
            if (ptr != IntPtr.Zero)
            {
                IPEndPoint point;
                IpAdapterUnicastAddress adapterAddress = (IpAdapterUnicastAddress) Marshal.PtrToStructure(ptr, typeof(IpAdapterUnicastAddress));
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
                informations.InternalAdd(new SystemUnicastIPAddressInformation(adapterAddress, point.Address));
                while (adapterAddress.next != IntPtr.Zero)
                {
                    adapterAddress = (IpAdapterUnicastAddress) Marshal.PtrToStructure(adapterAddress.next, typeof(IpAdapterUnicastAddress));
                    family = (adapterAddress.address.addressLength > 0x10) ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
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
                    informations.InternalAdd(new SystemUnicastIPAddressInformation(adapterAddress, point.Address));
                }
            }
            return informations;
        }

        public override IPAddress Address
        {
            get
            {
                return this.innerInfo.Address;
            }
        }

        public override long AddressPreferredLifetime
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return (long) this.adapterAddress.preferredLifetime;
            }
        }

        public override long AddressValidLifetime
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return (long) this.adapterAddress.validLifetime;
            }
        }

        public override long DhcpLeaseLifetime
        {
            get
            {
                return this.dhcpLeaseLifetime;
            }
        }

        public override System.Net.NetworkInformation.DuplicateAddressDetectionState DuplicateAddressDetectionState
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return this.adapterAddress.dadState;
            }
        }

        public override IPAddress IPv4Mask
        {
            get
            {
                if (this.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    return new IPAddress(0);
                }
                return this.ipv4Mask;
            }
        }

        public override bool IsDnsEligible
        {
            get
            {
                return this.innerInfo.IsDnsEligible;
            }
        }

        public override bool IsTransient
        {
            get
            {
                return this.innerInfo.IsTransient;
            }
        }

        public override System.Net.NetworkInformation.PrefixOrigin PrefixOrigin
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return this.adapterAddress.prefixOrigin;
            }
        }

        public override System.Net.NetworkInformation.SuffixOrigin SuffixOrigin
        {
            get
            {
                if (!ComNetOS.IsPostWin2K)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinXPRequired"));
                }
                return this.adapterAddress.suffixOrigin;
            }
        }
    }
}

