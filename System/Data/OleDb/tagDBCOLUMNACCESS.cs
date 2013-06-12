namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal struct tagDBCOLUMNACCESS
    {
        internal IntPtr pData;
        internal tagDBIDX columnid;
        internal IntPtr cbDataLen;
        internal int dwStatus;
        internal IntPtr cbMaxLen;
        internal IntPtr dwReserved;
        internal short wType;
        internal byte bPrecision;
        internal byte bScale;
    }
}

