namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngKeyCreationOptions
    {
        MachineKey = 0x20,
        None = 0,
        OverwriteExistingKey = 0x80
    }
}

