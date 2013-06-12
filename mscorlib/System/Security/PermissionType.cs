namespace System.Security
{
    using System;

    [Serializable]
    internal enum PermissionType
    {
        EnvironmentPermission = 10,
        FileDialogPermission = 11,
        FileIOPermission = 12,
        FullTrust = 7,
        ReflectionMemberAccess = 4,
        ReflectionPermission = 13,
        ReflectionRestrictedMemberAccess = 6,
        ReflectionTypeInfo = 2,
        SecurityAssert = 3,
        SecurityBindingRedirects = 8,
        SecurityControlEvidence = 0x10,
        SecurityControlPrincipal = 0x11,
        SecurityPermission = 14,
        SecuritySerialization = 5,
        SecuritySkipVerification = 1,
        SecurityUnmngdCodeAccess = 0,
        UIPermission = 9
    }
}

