namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBPROP
    {
        internal int dwPropertyID;
        internal int dwOptions;
        internal OleDbPropertyStatus dwStatus;
        internal tagDBIDX columnid;
        [MarshalAs(UnmanagedType.Struct)]
        internal object vValue;
        internal tagDBPROP()
        {
        }

        internal tagDBPROP(int propertyID, bool required, object value)
        {
            this.dwPropertyID = propertyID;
            this.dwOptions = required ? 0 : 1;
            this.vValue = value;
        }
    }
}

