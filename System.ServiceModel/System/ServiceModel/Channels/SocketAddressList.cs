namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SocketAddressList
    {
        internal const int maxAddresses = 50;
        private int count;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=50)]
        private System.ServiceModel.Channels.SocketAddress[] addresses;
        public System.ServiceModel.Channels.SocketAddress[] Addresses
        {
            get
            {
                return this.addresses;
            }
        }
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public SocketAddressList(System.ServiceModel.Channels.SocketAddress[] addresses, int count)
        {
            this.addresses = addresses;
            this.count = count;
        }

        public static ReadOnlyCollection<IPAddress> SortAddresses(Socket socket, IPAddress listenAddress, ReadOnlyCollection<IPAddress> addresses)
        {
            ReadOnlyCollection<IPAddress> onlys = null;
            if ((socket == null) || (addresses.Count <= 1))
            {
                return addresses;
            }
            CriticalAllocHandleSocketAddressList list = null;
            CriticalAllocHandleSocketAddressList list2 = null;
            try
            {
                int num;
                list = CriticalAllocHandleSocketAddressList.FromAddressList(addresses);
                list2 = CriticalAllocHandleSocketAddressList.FromAddressCount(0);
                int errorCode = 0;
                if (PeerWinsock.WSAIoctl(socket.Handle, -939524071, (IntPtr) list, list.Size, (IntPtr) list2, list2.Size, out num, IntPtr.Zero, IntPtr.Zero) == -1)
                {
                    errorCode = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SocketException(errorCode));
                }
                onlys = list2.ToAddresses();
            }
            finally
            {
                if (list != null)
                {
                    list.Dispose();
                }
                if (list2 != null)
                {
                    list2.Dispose();
                }
            }
            return onlys;
        }
    }
}

