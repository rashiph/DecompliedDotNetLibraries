namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBBINDING
    {
        internal IntPtr iOrdinal;
        internal IntPtr obValue;
        internal IntPtr obLength;
        internal IntPtr obStatus;
        internal IntPtr pTypeInfo;
        internal IntPtr pObject;
        internal IntPtr pBindExt;
        internal int dwPart;
        internal int dwMemOwner;
        internal int eParamIO;
        internal IntPtr cbMaxLen;
        internal int dwFlags;
        internal short wType;
        internal byte bPrecision;
        internal byte bScale;
        internal tagDBBINDING()
        {
        }
    }
}

