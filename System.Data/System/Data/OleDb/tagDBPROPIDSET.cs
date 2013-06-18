namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal struct tagDBPROPIDSET
    {
        internal IntPtr rgPropertyIDs;
        internal int cPropertyIDs;
        internal Guid guidPropertySet;
    }
}

