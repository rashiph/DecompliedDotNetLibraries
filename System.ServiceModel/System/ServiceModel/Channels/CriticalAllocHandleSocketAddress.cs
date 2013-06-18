namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class CriticalAllocHandleSocketAddress : CriticalAllocHandle
    {
        private int size;

        public static CriticalAllocHandleSocketAddress FromIPAddress(IPAddress input)
        {
            if (input == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("input");
            }
            CriticalAllocHandleSocketAddress address = null;
            address = FromSize(Marshal.SizeOf(typeof(System.ServiceModel.Channels.sockaddr_in6)));
            System.ServiceModel.Channels.sockaddr_in6 structure = new System.ServiceModel.Channels.sockaddr_in6(input);
            Marshal.StructureToPtr(structure, (IntPtr) address, false);
            return address;
        }

        public static CriticalAllocHandleSocketAddress FromSize(int size)
        {
            CriticalAllocHandleSocketAddress address = new CriticalAllocHandleSocketAddress();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                address.SetHandle(Marshal.AllocHGlobal(size));
                address.size = size;
            }
            return address;
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

