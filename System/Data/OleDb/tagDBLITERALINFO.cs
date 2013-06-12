namespace System.Data.OleDb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=2)]
    internal sealed class tagDBLITERALINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pwszLiteralValue;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pwszInvalidChars;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pwszInvalidStartingChars;
        internal int it;
        internal int fSupported;
        internal int cchMaxLen;
        internal tagDBLITERALINFO()
        {
        }
    }
}

