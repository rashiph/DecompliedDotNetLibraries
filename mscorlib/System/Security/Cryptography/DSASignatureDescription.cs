namespace System.Security.Cryptography
{
    using System;

    internal class DSASignatureDescription : SignatureDescription
    {
        public DSASignatureDescription()
        {
            base.KeyAlgorithm = "System.Security.Cryptography.DSACryptoServiceProvider";
            base.DigestAlgorithm = "System.Security.Cryptography.SHA1CryptoServiceProvider";
            base.FormatterAlgorithm = "System.Security.Cryptography.DSASignatureFormatter";
            base.DeformatterAlgorithm = "System.Security.Cryptography.DSASignatureDeformatter";
        }
    }
}

