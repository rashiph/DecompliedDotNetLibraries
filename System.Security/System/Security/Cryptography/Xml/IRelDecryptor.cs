namespace System.Security.Cryptography.Xml
{
    using System.IO;

    public interface IRelDecryptor
    {
        Stream Decrypt(EncryptionMethod encryptionMethod, KeyInfo keyInfo, Stream toDecrypt);
    }
}

