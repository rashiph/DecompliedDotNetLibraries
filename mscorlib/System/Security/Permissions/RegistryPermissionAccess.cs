namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum RegistryPermissionAccess
    {
        AllAccess = 7,
        Create = 4,
        NoAccess = 0,
        Read = 1,
        Write = 2
    }
}

