namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct TYPEDESC
    {
        public IntPtr lpValue;
        public short vt;
    }
}

