namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AdsSortKey
    {
        public IntPtr pszAttrType;
        public IntPtr pszReserved;
        public int fReverseOrder;
    }
}

