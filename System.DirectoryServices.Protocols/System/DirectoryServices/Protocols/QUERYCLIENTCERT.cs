namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate bool QUERYCLIENTCERT(IntPtr Connection, IntPtr trusted_CAs, ref IntPtr certificateHandle);
}

