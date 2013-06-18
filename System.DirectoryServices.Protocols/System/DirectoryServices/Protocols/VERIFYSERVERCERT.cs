namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool VERIFYSERVERCERT(IntPtr Connection, IntPtr pServerCert);
}

