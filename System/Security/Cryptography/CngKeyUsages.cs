namespace System.Security.Cryptography
{
    using System;

    [Flags]
    public enum CngKeyUsages
    {
        AllUsages = 0xffffff,
        Decryption = 1,
        KeyAgreement = 4,
        None = 0,
        Signing = 2
    }
}

