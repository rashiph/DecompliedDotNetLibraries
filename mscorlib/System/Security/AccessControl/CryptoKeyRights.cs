namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum CryptoKeyRights
    {
        ChangePermissions = 0x40000,
        Delete = 0x10000,
        FullControl = 0x1f019b,
        GenericAll = 0x10000000,
        GenericExecute = 0x20000000,
        GenericRead = -2147483648,
        GenericWrite = 0x40000000,
        ReadAttributes = 0x80,
        ReadData = 1,
        ReadExtendedAttributes = 8,
        ReadPermissions = 0x20000,
        Synchronize = 0x100000,
        TakeOwnership = 0x80000,
        WriteAttributes = 0x100,
        WriteData = 2,
        WriteExtendedAttributes = 0x10
    }
}

