namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct SecPkgContext_IssuerListInfoEx
    {
        public IntPtr aIssuers;
        public int cIssuers;
    }
}

