namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TOKEN_PRIVILEGE
    {
        internal uint PrivilegeCount;
        internal LUID_AND_ATTRIBUTES Privilege;
        internal static readonly uint Size;
        static TOKEN_PRIVILEGE()
        {
            Size = (uint) Marshal.SizeOf(typeof(TOKEN_PRIVILEGE));
        }
    }
}

