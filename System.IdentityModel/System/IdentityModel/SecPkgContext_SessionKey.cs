namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecPkgContext_SessionKey
    {
        internal uint SessionKeyLength;
        internal IntPtr Sessionkey;
        internal static readonly int Size;
        internal static readonly int SessionkeyOffset;
        static SecPkgContext_SessionKey()
        {
            Size = Marshal.SizeOf(typeof(SecPkgContext_SessionKey));
            SessionkeyOffset = (int) Marshal.OffsetOf(typeof(SecPkgContext_SessionKey), "Sessionkey");
        }
    }
}

