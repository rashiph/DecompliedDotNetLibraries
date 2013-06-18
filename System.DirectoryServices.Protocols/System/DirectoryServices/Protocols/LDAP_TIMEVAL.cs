namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class LDAP_TIMEVAL
    {
        public int tv_sec;
        public int tv_usec;
    }
}

