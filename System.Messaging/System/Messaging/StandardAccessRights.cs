namespace System.Messaging
{
    using System;

    [Flags]
    public enum StandardAccessRights
    {
        All = 0x1f0000,
        Delete = 0x10000,
        Execute = 0x20000,
        ModifyOwner = 0x80000,
        None = 0,
        Read = 0x20000,
        ReadSecurity = 0x20000,
        Required = 0xd0000,
        Synchronize = 0x100000,
        Write = 0x20000,
        WriteSecurity = 0x40000
    }
}

