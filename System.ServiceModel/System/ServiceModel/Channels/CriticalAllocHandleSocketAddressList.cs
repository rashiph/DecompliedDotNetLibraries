namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class CriticalAllocHandleSocketAddressList : CriticalAllocHandle
    {
        private int count;
        private int size;
        private CriticalAllocHandleSocketAddress[] socketHandles;

        public static CriticalAllocHandleSocketAddressList FromAddressCount(int count)
        {
            SocketAddressList structure = new SocketAddressList(new System.ServiceModel.Channels.SocketAddress[50], 0);
            CriticalAllocHandleSocketAddressList list2 = FromSize(Marshal.SizeOf(structure));
            list2.count = count;
            Marshal.StructureToPtr(structure, (IntPtr) list2, false);
            return list2;
        }

        public static CriticalAllocHandleSocketAddressList FromAddressList(ICollection<IPAddress> addresses)
        {
            if (addresses == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("addresses");
            }
            int count = addresses.Count;
            CriticalAllocHandleSocketAddress[] addressArray = new CriticalAllocHandleSocketAddress[50];
            SocketAddressList structure = new SocketAddressList(new System.ServiceModel.Channels.SocketAddress[50], count);
            int index = 0;
            foreach (IPAddress address in addresses)
            {
                if (index == 50)
                {
                    break;
                }
                addressArray[index] = CriticalAllocHandleSocketAddress.FromIPAddress(address);
                structure.Addresses[index].InitializeFromCriticalAllocHandleSocketAddress(addressArray[index]);
                index++;
            }
            CriticalAllocHandleSocketAddressList list2 = FromSize(Marshal.SizeOf(structure));
            list2.count = count;
            list2.socketHandles = addressArray;
            Marshal.StructureToPtr(structure, (IntPtr) list2, false);
            return list2;
        }

        private static CriticalAllocHandleSocketAddressList FromSize(int size)
        {
            CriticalAllocHandleSocketAddressList list = new CriticalAllocHandleSocketAddressList();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                list.SetHandle(Marshal.AllocHGlobal(size));
                list.size = size;
            }
            return list;
        }

        public ReadOnlyCollection<IPAddress> ToAddresses()
        {
            SocketAddressList list = (SocketAddressList) Marshal.PtrToStructure((IntPtr) this, typeof(SocketAddressList));
            IPAddress[] array = new IPAddress[list.Count];
            for (int i = 0; i < array.Length; i++)
            {
                if (list.Addresses[i].SockAddrLength != Marshal.SizeOf(typeof(System.ServiceModel.Channels.sockaddr_in6)))
                {
                    throw Fx.AssertAndThrow("sockAddressLength in SOCKET_ADDRESS expected to be valid");
                }
                array[i] = ((System.ServiceModel.Channels.sockaddr_in6) Marshal.PtrToStructure(list.Addresses[i].SockAddr, typeof(System.ServiceModel.Channels.sockaddr_in6))).ToIPAddress();
            }
            return Array.AsReadOnly<IPAddress>(array);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public int Size
        {
            get
            {
                return this.size;
            }
        }
    }
}

