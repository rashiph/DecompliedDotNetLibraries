namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Cryptography.Xml;

    public class X509AsymmetricSecurityKey : AsymmetricSecurityKey
    {
        private X509Certificate2 certificate;
        private AsymmetricAlgorithm privateKey;
        private bool privateKeyAvailabilityDetermined;
        private System.Security.Cryptography.X509Certificates.PublicKey publicKey;
        private object thisLock = new object();

        public X509AsymmetricSecurityKey(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            this.certificate = certificate;
        }

        public override byte[] DecryptKey(string algorithm, byte[] keyData)
        {
            if (this.PrivateKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("MissingPrivateKey")));
            }
            RSA privateKey = this.PrivateKey as RSA;
            if (privateKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PrivateKeyNotRSA")));
            }
            if (privateKey.KeyExchangeAlgorithm == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PrivateKeyExchangeNotSupported")));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return EncryptedXml.DecryptKey(keyData, privateKey, false);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    return EncryptedXml.DecryptKey(keyData, privateKey, true);
            }
            if (!this.IsSupportedAlgorithm(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
            }
            return EncryptedXml.DecryptKey(keyData, privateKey, true);
        }

        public override byte[] EncryptKey(string algorithm, byte[] keyData)
        {
            RSA key = this.PublicKey.Key as RSA;
            if (key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PublicKeyNotRSA")));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    return EncryptedXml.EncryptKey(keyData, key, false);

                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    return EncryptedXml.EncryptKey(keyData, key, true);
            }
            if (!this.IsSupportedAlgorithm(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
            }
            return EncryptedXml.EncryptKey(keyData, key, true);
        }

        public override AsymmetricAlgorithm GetAsymmetricAlgorithm(string algorithm, bool privateKey)
        {
            if (!privateKey)
            {
                switch (algorithm)
                {
                    case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                        if (!(this.PublicKey.Key is DSA))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPublicKeyMisMatch")));
                        }
                        return (this.PublicKey.Key as DSA);

                    case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                    case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                        if (!(this.PublicKey.Key is RSA))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPublicKeyMisMatch")));
                        }
                        return (this.PublicKey.Key as RSA);
                }
            }
            else
            {
                if (this.PrivateKey == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("MissingPrivateKey")));
                }
                if (string.IsNullOrEmpty(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
                }
                switch (algorithm)
                {
                    case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                        if (!(this.PrivateKey is DSA))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPrivateKeyMisMatch")));
                        }
                        return (this.PrivateKey as DSA);

                    case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                    case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                    case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                        if (!(this.PrivateKey is RSA))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPrivateKeyMisMatch")));
                        }
                        return (this.PrivateKey as RSA);
                }
                if (!this.IsSupportedAlgorithm(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
                }
                return this.PrivateKey;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "CreateDigest" })));
                }
                return algorithm2;
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                    return CryptoHelper.NewSha1HashAlgorithm();

                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                    return CryptoHelper.NewSha256HashAlgorithm();
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
        }

        public override AsymmetricSignatureDeformatter GetSignatureDeformatter(string algorithm)
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
                    return description.CreateDeformatter(this.PublicKey.Key);
                }
                try
                {
                    AsymmetricSignatureDeformatter deformatter = algorithmFromConfig as AsymmetricSignatureDeformatter;
                    if (deformatter != null)
                    {
                        deformatter.SetKey(this.PublicKey.Key);
                        return deformatter;
                    }
                }
                catch (InvalidCastException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPublicKeyMisMatch"), exception));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureDeformatter" })));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                {
                    DSA key = this.PublicKey.Key as DSA;
                    if (key == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PublicKeyNotDSA")));
                    }
                    return new DSASignatureDeformatter(key);
                }
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                {
                    RSA rsa = this.PublicKey.Key as RSA;
                    if (rsa == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PublicKeyNotRSA")));
                    }
                    return new RSAPKCS1SignatureDeformatter(rsa);
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
        }

        public override AsymmetricSignatureFormatter GetSignatureFormatter(string algorithm)
        {
            if (this.PrivateKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("MissingPrivateKey")));
            }
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
                    return description.CreateFormatter(this.PrivateKey);
                }
                try
                {
                    AsymmetricSignatureFormatter formatter = algorithmFromConfig as AsymmetricSignatureFormatter;
                    if (formatter != null)
                    {
                        formatter.SetKey(this.PrivateKey);
                        return formatter;
                    }
                }
                catch (InvalidCastException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AlgorithmAndPrivateKeyMisMatch"), exception));
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnsupportedAlgorithmForCryptoOperation", new object[] { algorithm, "GetSignatureFormatter" })));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                {
                    DSA privateKey = this.PrivateKey as DSA;
                    if (privateKey == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PrivateKeyNotDSA")));
                    }
                    return new DSASignatureFormatter(privateKey);
                }
                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                {
                    RSA key = this.PrivateKey as RSA;
                    if (key == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PrivateKeyNotRSA")));
                    }
                    return new RSAPKCS1SignatureFormatter(key);
                }
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                {
                    RSACryptoServiceProvider provider = this.PrivateKey as RSACryptoServiceProvider;
                    if (provider == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("PrivateKeyNotRSA")));
                    }
                    CspParameters parameters = new CspParameters {
                        ProviderType = 0x18,
                        KeyContainerName = provider.CspKeyContainerInfo.KeyContainerName,
                        KeyNumber = (int) provider.CspKeyContainerInfo.KeyNumber
                    };
                    if (provider.CspKeyContainerInfo.MachineKeyStore)
                    {
                        parameters.Flags = CspProviderFlags.UseMachineKeyStore;
                    }
                    return new RSAPKCS1SignatureFormatter(new RSACryptoServiceProvider(parameters));
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("UnsupportedCryptoAlgorithm", new object[] { algorithm })));
        }

        public override bool HasPrivateKey()
        {
            return (this.PrivateKey != null);
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
                algorithm = null;
            }
            if (algorithmFromConfig != null)
            {
                return ((algorithmFromConfig is SignatureDescription) || (algorithmFromConfig is AsymmetricAlgorithm));
            }
            switch (algorithm)
            {
                case "http://www.w3.org/2000/09/xmldsig#dsa-sha1":
                    return (this.PublicKey.Key is DSA);

                case "http://www.w3.org/2000/09/xmldsig#rsa-sha1":
                case "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256":
                case "http://www.w3.org/2001/04/xmlenc#rsa-1_5":
                case "http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p":
                    return (this.PublicKey.Key is RSA);
            }
            return false;
        }

        public override bool IsSymmetricAlgorithm(string algorithm)
        {
            return CryptoHelper.IsSymmetricAlgorithm(algorithm);
        }

        public override int KeySize
        {
            get
            {
                return this.PublicKey.Key.KeySize;
            }
        }

        private AsymmetricAlgorithm PrivateKey
        {
            get
            {
                if (!this.privateKeyAvailabilityDetermined)
                {
                    lock (this.ThisLock)
                    {
                        if (!this.privateKeyAvailabilityDetermined)
                        {
                            this.privateKey = this.certificate.PrivateKey;
                            this.privateKeyAvailabilityDetermined = true;
                        }
                    }
                }
                return this.privateKey;
            }
        }

        private System.Security.Cryptography.X509Certificates.PublicKey PublicKey
        {
            get
            {
                if (this.publicKey == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.publicKey == null)
                        {
                            this.publicKey = this.certificate.PublicKey;
                        }
                    }
                }
                return this.publicKey;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }
    }
}

