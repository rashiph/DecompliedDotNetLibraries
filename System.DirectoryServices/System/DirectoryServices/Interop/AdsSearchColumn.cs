namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AdsSearchColumn
    {
        public IntPtr pszAttrName;
        public int dwADsType;
        public unsafe AdsValue* pADsValues;
        public int dwNumValues;
        public IntPtr hReserved;
    }
}

