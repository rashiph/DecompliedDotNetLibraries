namespace System.DirectoryServices
{
    using System;

    [Flags]
    public enum DirectoryServicesPermissionAccess
    {
        Browse = 2,
        None = 0,
        Write = 6
    }
}

