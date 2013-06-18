namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using System;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;

    internal class InfoCardAsymmetricCrypto : AsymmetricSecurityKey, IDisposable
    {
        private InfoCardRSACryptoProvider m_rsa;

        public InfoCardAsymmetricCrypto(AsymmetricCryptoHandle cryptoHandle)
        {
            this.m_rsa = new InfoCardRSACryptoProvider(cryptoHandle);
        }

        public override byte[] DecryptKey(string algorithmUri, byte[] keyData)
        {
            AsymmetricKeyExchangeDeformatter deformatter;
            switch (algorithmUri)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    deformatter = new InfoCardRSAPKCS1KeyExchangeDeformatter(this.m_rsa);
                    return deformatter.DecryptKeyExchange(keyData);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    deformatter = new InfoCardRSAOAEPKeyExchangeDeformatter(this.m_rsa);
                    return deformatter.DecryptKeyExchange(keyData);
            }
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
        }

        public void Dispose()
        {
            this.m_rsa.Dispose();
            this.m_rsa = null;
        }

        public override byte[] EncryptKey(string algorithmUri, byte[] keyData)
        {
            AsymmetricKeyExchangeFormatter formatter;
            switch (algorithmUri)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    formatter = new InfoCardRSAPKCS1KeyExchangeFormatter(this.m_rsa);
                    return formatter.CreateKeyExchange(keyData);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    formatter = new InfoCardRSAOAEPKeyExchangeFormatter(this.m_rsa);
                    return formatter.CreateKeyExchange(keyData);
            }
            throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
        }

        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithmUri, bool privateKey)
        {
            string str;
            if (((str = algorithmUri) == null) || (((str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1") && (str != "http://www.w3.org/2001/04/xmlenc#rsa-1_5")) && (str != "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p")))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return this.m_rsa;
        }

        public override HashAlgorithm GetHashAlgorithmForSignature(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return new SHA1Managed();
        }

        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return new InfoCardRSAPKCS1SignatureDeformatter(this.m_rsa);
        }

        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || (str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1"))
            {
                throw InfoCardTrace.ThrowHelperError(new NotSupportedException(Microsoft.InfoCards.SR.GetString("ClientUnsupportedCryptoAlgorithm", new object[] { algorithmUri })));
            }
            return new InfoCardRSAPKCS1SignatureFormatter(this.m_rsa);
        }

        public override bool HasPrivateKey()
        {
            return true;
        }

        public override bool IsAsymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsAsymmetricAlgorithm(algorithmUri);
        }

        public override bool IsSupportedAlgorithm(string algorithmUri)
        {
            string str;
            if (((str = algorithmUri) == null) || ((!(str == "http://www.w3.org/2000/09/xmldsig#rsa-sha1") && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-1_5")) && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p")))
            {
                return false;
            }
            return true;
        }

        public override bool IsSymmetricAlgorithm(string algorithmUri)
        {
            return InfoCardCryptoHelper.IsSymmetricAlgorithm(algorithmUri);
        }

        public override int KeySize
        {
            get
            {
                return this.m_rsa.KeySize;
            }
        }
    }
}

