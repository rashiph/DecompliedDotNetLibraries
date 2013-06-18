namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct TOKEN_GROUPS
    {
        internal uint GroupCount;
        internal SID_AND_ATTRIBUTES Groups;
    }
}

