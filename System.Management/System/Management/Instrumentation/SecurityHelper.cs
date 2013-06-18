namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    internal sealed class SecurityHelper
    {
        internal static readonly SecurityPermission UnmanagedCode = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
    }
}

