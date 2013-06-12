namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum FileIOPermissionAccess
    {
        AllAccess = 15,
        Append = 4,
        NoAccess = 0,
        PathDiscovery = 8,
        Read = 1,
        Write = 2
    }
}

