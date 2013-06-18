namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DS_REPL_CURSORS
    {
        public int cNumCursors;
        public int reserved;
    }
}

