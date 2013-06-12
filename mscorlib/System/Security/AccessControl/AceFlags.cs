namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum AceFlags : byte
    {
        AuditFlags = 0xc0,
        ContainerInherit = 2,
        FailedAccess = 0x80,
        InheritanceFlags = 15,
        Inherited = 0x10,
        InheritOnly = 8,
        None = 0,
        NoPropagateInherit = 4,
        ObjectInherit = 1,
        SuccessfulAccess = 0x40
    }
}

