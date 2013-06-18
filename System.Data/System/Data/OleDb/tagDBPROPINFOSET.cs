namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBPROPINFOSET
    {
        internal IntPtr rgPropertyInfos;
        internal int cPropertyInfos;
        internal Guid guidPropertySet;
        internal tagDBPROPINFOSET()
        {
        }
    }
}

