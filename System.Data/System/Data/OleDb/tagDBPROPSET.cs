namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBPROPSET
    {
        internal IntPtr rgProperties;
        internal int cProperties;
        internal Guid guidPropertySet;
        internal tagDBPROPSET()
        {
        }

        internal tagDBPROPSET(int propertyCount, Guid propertySet)
        {
            this.cProperties = propertyCount;
            this.guidPropertySet = propertySet;
        }
    }
}

