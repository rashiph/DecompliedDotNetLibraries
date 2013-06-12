namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBCOLUMNINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pwszName;
        internal IntPtr pTypeInfo = IntPtr.Zero;
        internal IntPtr iOrdinal = IntPtr.Zero;
        internal int dwFlags;
        internal IntPtr ulColumnSize = IntPtr.Zero;
        internal short wType;
        internal byte bPrecision;
        internal byte bScale;
        internal tagDBIDX columnid;
        internal tagDBCOLUMNINFO()
        {
        }
    }
}

