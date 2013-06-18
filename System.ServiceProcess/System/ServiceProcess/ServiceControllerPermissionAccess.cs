namespace System.ServiceProcess
{
    using System;

    [Flags]
    public enum ServiceControllerPermissionAccess
    {
        Browse = 2,
        Control = 6,
        None = 0
    }
}

