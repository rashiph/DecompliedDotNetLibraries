namespace System.Runtime.InteropServices
{
    using System;

    [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode), Obsolete("Use System.Runtime.InteropServices.ComTypes.BINDPTR instead. http://go.microsoft.com/fwlink/?linkid=14202", false)]
    public struct BINDPTR
    {
        [FieldOffset(0)]
        public IntPtr lpfuncdesc;
        [FieldOffset(0)]
        public IntPtr lptcomp;
        [FieldOffset(0)]
        public IntPtr lpvardesc;
    }
}

