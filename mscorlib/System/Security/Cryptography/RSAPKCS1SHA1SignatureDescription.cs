namespace System.Security.Cryptography
{
    using System;

    internal class RSAPKCS1SHA1SignatureDescription : SignatureDescription
    {
        public RSAPKCS1SHA1SignatureDescription()
        {
            base.KeyAlgorithm = "System.Security.Cryptography.RSACryptoServiceProvider";
            base.DigestAlgorithm = "System.Security.Cryptography.SHA1CryptoServiceProvider";
            base.FormatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureFormatter";
            base.DeformatterAlgorithm = "System.Security.Cryptography.RSAPKCS1SignatureDeformatter";
        }

        public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
        {
            AsymmetricSignatureDeformatter deformatter = (AsymmetricSignatureDeformatter) CryptoConfig.CreateFromName(base.DeformatterAlgorithm);
            deformatter.SetKey(key);
            deformatter.SetHashAlgorithm("SHA1");
            return deformatter;
        }
    }
}

