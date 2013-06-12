namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.CONNECTDATA instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct CONNECTDATA
    {
        [MarshalAs(UnmanagedType.Interface)]
        public object pUnk;
        public int dwCookie;
    }
}

