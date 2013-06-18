namespace System.Messaging
{
    using System;

    [Flags]
    public enum GenericAccessRights
    {
        All = 0x10000000,
        Execute = 0x20000000,
        None = 0,
        Read = -2147483648,
        Write = 0x40000000
    }
}

