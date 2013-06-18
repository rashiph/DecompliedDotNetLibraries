namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;

    public sealed class RsaSecurityKey : AsymmetricSecurityKey
    {
        private PrivateKeyStatus privateKeyStatus;
        private readonly RSA rsa;

        public RsaSecurityKey(RSA rsa)
        {
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            }
            this.rsa = rsa;
        }

        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return EncryptedXml.DecryptKey(keyData, this.rsa, false);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    return EncryptedXml.DecryptKey(keyData, this.rsa, true);
            }
            if (!this.IsSupportedAlgorithm(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "DecryptKey" })));
            }
            return EncryptedXml.DecryptKey(keyData, this.rsa, false);
        }

        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return EncryptedXml.EncryptKey(keyData, this.rsa, false);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    return EncryptedXml.EncryptKey(keyData, this.rsa, true);
            }
            if (!this.IsSupportedAlgorithm(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "EncryptKey" })));
            }
            return EncryptedXml.EncryptKey(keyData, this.rsa, false);
        }

        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithm, bool requiresPrivateKey)
        {
            if (requiresPrivateKey && !this.HasPrivateKey())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("NoPrivateKeyAvailable")));
            }
            return this.rsa;
        }

        public override HashAlgorithm GetHashAlgorithmForSignature(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
            }
            object algorithmFromConfig = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if (description != null)
                {
                    return description.CreateDigest();
                }
                HashAlgorithm algorithm2 = algorithmFromConfig as HashAlgorithm;
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
                }
                return algorithm2;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                    return CryptoHelper.NewSha1HashAlgorithm();

                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    return CryptoHelper.NewSha256HashAlgorithm();
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetHashAlgorithmForSignature" })));
        }

        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithm)
        {
            string str;
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
            }
            object algorithmFromConfig = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if (description != null)
                {
                    return description.CreateDeformatter(this.rsa);
                }
                try
                {
                    AsymmetricSignatureDeformatter deformatter = algorithmFromConfig as AsymmetricSignatureDeformatter;
                    if (deformatter != null)
                    {
                        deformatter.SetKey(this.rsa);
                        return deformatter;
                    }
                }
                catch (InvalidCastException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndKeyMisMatch", new object[] { algorithm }), exception));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureDeformatter" })));
            }
            if (((str = algorithm) == null) || ((str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1") && (str != "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureDeformatter" })));
            }
            return new RSAPKCS1SignatureDeformatter(this.rsa);
        }

        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithm)
        {
            string str;
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
            }
            object algorithmFromConfig = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            if (algorithmFromConfig != null)
            {
                SignatureDescription description = algorithmFromConfig as SignatureDescription;
                if (description != null)
                {
                    return description.CreateFormatter(this.rsa);
                }
                try
                {
                    AsymmetricSignatureFormatter formatter = algorithmFromConfig as AsymmetricSignatureFormatter;
                    if (formatter != null)
                    {
                        formatter.SetKey(this.rsa);
                        return formatter;
                    }
                }
                catch (InvalidCastException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndKeyMisMatch"), exception));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureFormatter" })));
            }
            if (((str = algorithm) == null) || ((str != "http://www.w3.org/2000/09/xmldsig#rsa-sha1") && (str != "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256")))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureFormatter" })));
            }
            return new RSAPKCS1SignatureFormatter(this.rsa);
        }

        public override bool HasPrivateKey()
        {
            if (this.privateKeyStatus == PrivateKeyStatus.AvailabilityNotDetermined)
            {
                RSACryptoServiceProvider rsa = this.rsa as RSACryptoServiceProvider;
                if (rsa != null)
                {
                    this.privateKeyStatus = rsa.PublicOnly ? PrivateKeyStatus.DoesNotHavePrivateKey : PrivateKeyStatus.HasPrivateKey;
                }
                else
                {
                    try
                    {
                        byte[] rgb = new byte[20];
                        this.rsa.DecryptValue(rgb);
                        this.privateKeyStatus = PrivateKeyStatus.HasPrivateKey;
                    }
                    catch (CryptographicException)
                    {
                        this.privateKeyStatus = PrivateKeyStatus.DoesNotHavePrivateKey;
                    }
                }
            }
            return (this.privateKeyStatus == PrivateKeyStatus.HasPrivateKey);
        }

        public override bool IsAsymmetricAlgorithm(string algorithm)
        {
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
            }
            return CryptoHelper.IsAsymmetricAlgorithm(algorithm);
        }

        public override bool IsSupportedAlgorithm(string algorithm)
        {
            string str;
            if (string.IsNullOrEmpty(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
            }
            object algorithmFromConfig = null;
            try
            {
                algorithmFromConfig = CryptoHelper.GetAlgorithmFromConfig(algorithm);
            }
            catch (InvalidOperationException)
            {
                algorithmFromConfig = null;
            }
            if (algorithmFromConfig != null)
            {
                return ((algorithmFromConfig is SignatureDescription) || (algorithmFromConfig is AsymmetricAlgorithm));
            }
            if (((str = algorithm) == null) || ((!(str == "http://www.w3.org/2001/04/xmlenc#rsa-1_5") && !(str == "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p")) && (!(str == "http://www.w3.org/2000/09/xmldsig#rsa-sha1") && !(str == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"))))
            {
                return false;
            }
            return true;
        }

        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.IsSymmetricAlgorithm(algorithm);
        }

        public override int KeySize
        {
            get
            {
                return this.rsa.KeySize;
            }
        }

        private enum PrivateKeyStatus
        {
            AvailabilityNotDetermined,
            HasPrivateKey,
            DoesNotHavePrivateKey
        }
    }
}

