namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPL_KCC_DSA_FAILURES
    {
        public int cNumEntries;
        public int dwReserved;
    }
}

