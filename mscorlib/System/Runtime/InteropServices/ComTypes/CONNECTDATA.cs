namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct CONNECTDATA
    {
        [MarshalAs(UnmanagedType.Interface)]
        public object pUnk;
        public int dwCookie;
    }
}

