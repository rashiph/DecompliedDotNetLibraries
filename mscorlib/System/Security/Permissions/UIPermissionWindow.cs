namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum UIPermissionWindow
    {
        NoWindows,
        SafeSubWindows,
        SafeTopLevelWindows,
        AllWindows
    }
}

