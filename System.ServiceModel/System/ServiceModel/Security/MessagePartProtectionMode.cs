namespace System.ServiceModel.Security
{
    using System;

    internal enum MessagePartProtectionMode
    {
        None,
        Sign,
        Encrypt,
        SignThenEncrypt,
        EncryptThenSign
    }
}

