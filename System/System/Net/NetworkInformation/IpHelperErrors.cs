namespace System.Net.NetworkInformation
{
    using System;
    using System.Net.Sockets;

    internal class IpHelperErrors
    {
        internal const uint ErrorBufferOverflow = 0x6f;
        internal const uint ErrorInsufficientBuffer = 0x7a;
        internal const uint ErrorInvalidData = 13;
        internal const uint ErrorInvalidFunction = 1;
        internal const uint ErrorInvalidParameter = 0x57;
        internal const uint ErrorNoData = 0xe8;
        internal const uint ErrorNoSuchDevice = 2;
        internal const uint ErrorNotFound = 0x490;
        internal const uint Pending = 0x3e5;
        internal const uint Success = 0;

        internal static void CheckFamilyUnspecified(AddressFamily family)
        {
            if (((family != AddressFamily.InterNetwork) && (family != AddressFamily.InterNetworkV6)) && (family != AddressFamily.Unspecified))
            {
                throw new ArgumentException(SR.GetString("net_invalidversion"), "family");
            }
        }
    }
}

