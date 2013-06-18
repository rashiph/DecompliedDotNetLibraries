namespace System.ServiceModel.Security
{
    using System;

    public enum MessageProtectionOrder
    {
        SignBeforeEncrypt,
        SignBeforeEncryptAndEncryptSignature,
        EncryptBeforeSign
    }
}

