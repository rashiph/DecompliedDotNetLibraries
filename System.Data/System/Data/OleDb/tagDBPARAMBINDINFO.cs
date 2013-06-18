namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal struct tagDBPARAMBINDINFO
    {
        internal IntPtr pwszDataSourceType;
        internal IntPtr pwszName;
        internal IntPtr ulParamSize;
        internal int dwFlags;
        internal byte bPrecision;
        internal byte bScale;
    }
}

