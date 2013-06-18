namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal sealed class LdapControl
    {
        public IntPtr ldctl_oid = IntPtr.Zero;
        public berval ldctl_value;
        public bool ldctl_iscritical;
    }
}

