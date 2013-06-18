namespace System.ServiceModel.Security
{
    using System;
    using System.Security.Authentication.ExtendedProtection;

    internal interface ISspiNegotiation : IDisposable
    {
        byte[] Decrypt(byte[] encryptedData);
        byte[] Encrypt(byte[] data);
        byte[] GetOutgoingBlob(byte[] incomingBlob, ChannelBinding channelbinding, ExtendedProtectionPolicy protectionPolicy);
        string GetRemoteIdentityName();

        DateTime ExpirationTimeUtc { get; }

        bool IsCompleted { get; }

        bool IsValidContext { get; }

        string KeyEncryptionAlgorithm { get; }
    }
}

