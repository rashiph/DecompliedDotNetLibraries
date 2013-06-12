namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ICspAsymmetricAlgorithm
    {
        byte[] ExportCspBlob(bool includePrivateParameters);
        void ImportCspBlob(byte[] rawData);

        System.Security.Cryptography.CspKeyContainerInfo CspKeyContainerInfo { get; }
    }
}

