namespace System.Security.Cryptography
{
    using System;

    [Flags]
    internal enum CngKeyTypes
    {
        MachineKey = 0x20,
        None = 0
    }
}

