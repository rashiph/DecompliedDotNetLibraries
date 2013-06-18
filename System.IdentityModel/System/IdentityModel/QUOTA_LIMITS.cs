namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct QUOTA_LIMITS
    {
        internal IntPtr PagedPoolLimit;
        internal IntPtr NonPagedPoolLimit;
        internal IntPtr MinimumWorkingSetSize;
        internal IntPtr MaximumWorkingSetSize;
        internal IntPtr PagefileLimit;
        internal IntPtr TimeLimit;
    }
}

