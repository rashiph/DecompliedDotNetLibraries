namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct SID_AND_ATTRIBUTES
    {
        internal IntPtr Sid;
        internal uint Attributes;
        internal static readonly long SizeOf;
        static SID_AND_ATTRIBUTES()
        {
            SizeOf = Marshal.SizeOf(typeof(SID_AND_ATTRIBUTES));
        }
    }
}

