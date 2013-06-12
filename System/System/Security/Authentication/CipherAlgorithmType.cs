namespace System.Security.Authentication
{
    using System;

    public enum CipherAlgorithmType
    {
        Aes = 0x6611,
        Aes128 = 0x660e,
        Aes192 = 0x660f,
        Aes256 = 0x6610,
        Des = 0x6601,
        None = 0,
        Null = 0x6000,
        Rc2 = 0x6602,
        Rc4 = 0x6801,
        TripleDes = 0x6603
    }
}

