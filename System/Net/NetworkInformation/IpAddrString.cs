namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IpAddrString
    {
        internal IntPtr Next;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x10)]
        internal string IpAddress;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=0x10)]
        internal string IpMask;
        internal uint Context;
        internal IPAddressCollection ToIPAddressCollection()
        {
            IpAddrString str = this;
            IPAddressCollection addresss = new IPAddressCollection();
            if (str.IpAddress.Length != 0)
            {
                addresss.InternalAdd(IPAddress.Parse(str.IpAddress));
            }
            while (str.Next != IntPtr.Zero)
            {
                str = (IpAddrString) Marshal.PtrToStructure(str.Next, typeof(IpAddrString));
                if (str.IpAddress.Length != 0)
                {
                    addresss.InternalAdd(IPAddress.Parse(str.IpAddress));
                }
            }
            return addresss;
        }

        internal ArrayList ToIPExtendedAddressArrayList()
        {
            IpAddrString str = this;
            ArrayList list = new ArrayList();
            if (str.IpAddress.Length != 0)
            {
                IPAddress address = IPAddress.Parse(str.IpAddress);
                list.Add(new IPExtendedAddress(address, IPAddress.Parse(str.IpMask)));
            }
            while (str.Next != IntPtr.Zero)
            {
                str = (IpAddrString) Marshal.PtrToStructure(str.Next, typeof(IpAddrString));
                if (str.IpAddress.Length != 0)
                {
                    IPAddress introduced3 = IPAddress.Parse(str.IpAddress);
                    list.Add(new IPExtendedAddress(introduced3, IPAddress.Parse(str.IpMask)));
                }
            }
            return list;
        }

        internal GatewayIPAddressInformationCollection ToIPGatewayAddressCollection()
        {
            IpAddrString str = this;
            GatewayIPAddressInformationCollection informations = new GatewayIPAddressInformationCollection();
            if (str.IpAddress.Length != 0)
            {
                informations.InternalAdd(new SystemGatewayIPAddressInformation(IPAddress.Parse(str.IpAddress)));
            }
            while (str.Next != IntPtr.Zero)
            {
                str = (IpAddrString) Marshal.PtrToStructure(str.Next, typeof(IpAddrString));
                if (str.IpAddress.Length != 0)
                {
                    informations.InternalAdd(new SystemGatewayIPAddressInformation(IPAddress.Parse(str.IpAddress)));
                }
            }
            return informations;
        }
    }
}

