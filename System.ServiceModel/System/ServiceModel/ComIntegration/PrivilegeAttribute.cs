namespace System.ServiceModel.ComIntegration
{
    using System;

    [Flags]
    internal enum PrivilegeAttribute : uint
    {
        SE_PRIVILEGE_DISABLED = 0,
        SE_PRIVILEGE_ENABLED = 2,
        SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1,
        SE_PRIVILEGE_REMOVED = 4,
        SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000
    }
}

