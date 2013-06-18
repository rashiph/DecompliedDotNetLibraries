namespace System.DirectoryServices
{
    using System;

    [Flags]
    public enum AuthenticationTypes
    {
        Anonymous = 0x10,
        Delegation = 0x100,
        Encryption = 2,
        FastBind = 0x20,
        None = 0,
        ReadonlyServer = 4,
        Sealing = 0x80,
        Secure = 1,
        SecureSocketsLayer = 2,
        ServerBind = 0x200,
        Signing = 0x40
    }
}

