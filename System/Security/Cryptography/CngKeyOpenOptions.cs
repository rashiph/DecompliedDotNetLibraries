namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngKeyOpenOptions
    {
        MachineKey = 0x20,
        None = 0,
        Silent = 0x40,
        UserKey = 0
    }
}

