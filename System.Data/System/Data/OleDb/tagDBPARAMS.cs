namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBPARAMS
    {
        internal IntPtr pData;
        internal int cParamSets;
        internal IntPtr hAccessor;
        internal tagDBPARAMS()
        {
        }
    }
}

