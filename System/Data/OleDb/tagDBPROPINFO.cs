namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBPROPINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pwszDescription;
        internal int dwPropertyID;
        internal int dwFlags;
        internal short vtType;
        [MarshalAs(UnmanagedType.Struct)]
        internal object vValue;
        internal tagDBPROPINFO()
        {
        }
    }
}

