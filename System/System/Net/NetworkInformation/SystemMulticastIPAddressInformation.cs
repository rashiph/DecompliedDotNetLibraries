namespace System.Net.NetworkInformation
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    internal class SystemMulticastIPAddressInformation : MulticastIPAddressInformation
    {
        private IpAdapterAddress adapterAddress;
        private SystemIPAddressInformation innerInfo;

        private SystemMulticastIPAddressInformation()
        {
        }

        internal SystemMulticastIPAddressInformation(IpAdapterAddress adapterAddress, IPAddress ipAddress)
        {
            this.innerInfo = new SystemIPAddressInformation(adapterAddress, ipAddress);
            this.adapterAddress = adapterAddress;
        }

        internal static MulticastIPAddressInformationCollection ToAddressInformationCollection(IntPtr ptr)
        {
            MulticastIPAddressInformationCollection informations = new MulticastIPAddressInformationCollection();
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
                informations.InternalAdd(new SystemMulticastIPAddressInformation(adapterAddress, point.Address));
                while (adapterAddress.next != IntPtr.Zero)
                {
                    adapterAddress = (IpAdapterAddress) Marshal.PtrToStructure(adapterAddress.next, typeof(IpAdapterAddress));
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
                    informations.InternalAdd(new SystemMulticastIPAddressInformation(adapterAddress, point.Address));
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
                return 0L;
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
                return 0L;
            }
        }

        public override long DhcpLeaseLifetime
        {
            get
            {
                return 0L;
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
                return System.Net.NetworkInformation.DuplicateAddressDetectionState.Invalid;
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
                return System.Net.NetworkInformation.PrefixOrigin.Other;
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
                return System.Net.NetworkInformation.SuffixOrigin.Other;
            }
        }
    }
}

