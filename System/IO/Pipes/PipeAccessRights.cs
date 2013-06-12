namespace System.IO.Pipes
{
    using System;

    [Flags]
    public enum PipeAccessRights
    {
        AccessSystemSecurity = 0x1000000,
        ChangePermissions = 0x40000,
        CreateNewInstance = 4,
        Delete = 0x10000,
        FullControl = 0x1f019f,
        Read = 0x20089,
        ReadAttributes = 0x80,
        ReadData = 1,
        ReadExtendedAttributes = 8,
        ReadPermissions = 0x20000,
        ReadWrite = 0x2019b,
        Synchronize = 0x100000,
        TakeOwnership = 0x80000,
        Write = 0x112,
        WriteAttributes = 0x100,
        WriteData = 2,
        WriteExtendedAttributes = 0x10
    }
}

