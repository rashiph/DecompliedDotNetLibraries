namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeAddrInfo : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string WS2_32 = "ws2_32.dll";

        private SafeFreeAddrInfo() : base(true)
        {
        }

        internal static int GetAddrInfo(string nodename, string servicename, ref AddressInfo hints, out SafeFreeAddrInfo outAddrInfo)
        {
            return UnsafeNclNativeMethods.SafeNetHandlesXPOrLater.getaddrinfo(nodename, servicename, ref hints, out outAddrInfo);
        }

        protected override bool ReleaseHandle()
        {
            UnsafeNclNativeMethods.SafeNetHandlesXPOrLater.freeaddrinfo(base.handle);
            return true;
        }
    }
}

