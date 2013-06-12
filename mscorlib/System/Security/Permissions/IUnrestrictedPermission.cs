namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface IUnrestrictedPermission
    {
        bool IsUnrestricted();
    }
}

