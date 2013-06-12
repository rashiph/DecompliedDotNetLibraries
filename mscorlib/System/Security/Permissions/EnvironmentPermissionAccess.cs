namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum EnvironmentPermissionAccess
    {
        NoAccess,
        Read,
        Write,
        AllAccess
    }
}

