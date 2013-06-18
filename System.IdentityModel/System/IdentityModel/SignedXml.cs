namespace System.IdentityModel
{
    using System;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class SignedXml : ISignatureValueSecurityElement, ISecurityElement
    {
        internal const string DefaultPrefix = "";
        private DictionaryManager dictionaryManager;
        private readonly System.IdentityModel.Signature signature;
        private System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer;
        private System.IdentityModel.TransformFactory transformFactory;

        public SignedXml(DictionaryManager dictionaryManager, System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer) : this(new StandardSignedInfo(dictionaryManager), dictionaryManager, tokenSerializer)
        {
        }

        internal SignedXml(SignedInfo signedInfo, DictionaryManager dictionaryManager, System.IdentityModel.Selectors.SecurityTokenSerializer tokenSerializer)
        {
            if (signedInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("signedInfo"));
            }
            if (dictionaryManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dictionaryManager");
            }
            if (tokenSerializer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenSerializer");
            }
            this.transformFactory = StandardTransformFactory.Instance;
            this.tokenSerializer = tokenSerializer;
            this.signature = new System.IdentityModel.Signature(this, signedInfo);
            this.dictionaryManager = dictionaryManager;
        }

        public void CompleteSignatureVerification()
        {
            this.Signature.SignedInfo.EnsureAllReferencesVerified();
        }

        public void ComputeSignature(SecurityKey signingKey)
        {
            string signatureMethod = this.Signature.SignedInfo.SignatureMethod;
            SymmetricSecurityKey key = signingKey as SymmetricSecurityKey;
            if (key != null)
            {
                using (KeyedHashAlgorithm algorithm = key.GetKeyedHashAlgorithm(signatureMethod))
                {
                    if (algorithm == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToCreateKeyedHashAlgorithm", new object[] { key, signatureMethod })));
                    }
                    this.ComputeSignature(algorithm);
                    return;
                }
            }
            AsymmetricSecurityKey key2 = signingKey as AsymmetricSecurityKey;
            if (key2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnknownICryptoType", new object[] { signingKey })));
            }
            using (HashAlgorithm algorithm2 = key2.GetHashAlgorithmForSignature(signatureMethod))
            {
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToCreateHashAlgorithmFromAsymmetricCrypto", new object[] { signatureMethod, key2 })));
                }
                AsymmetricSignatureFormatter signatureFormatter = key2.GetSignatureFormatter(signatureMethod);
                if (signatureFormatter == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnableToCreateSignatureFormatterFromAsymmetricCrypto", new object[] { signatureMethod, key2 })));
                }
                this.ComputeSignature(algorithm2, signatureFormatter, signatureMethod);
            }
        }

        private void ComputeSignature(KeyedHashAlgorithm hash)
        {
            this.Signature.SignedInfo.ComputeReferenceDigests();
            this.Signature.SignedInfo.ComputeHash(hash);
            byte[] signatureValue = hash.Hash;
            this.Signature.SetSignatureValue(signatureValue);
        }

        private void ComputeSignature(HashAlgorithm hash, AsymmetricSignatureFormatter formatter, string signatureMethod)
        {
            byte[] buffer;
            this.Signature.SignedInfo.ComputeReferenceDigests();
            this.Signature.SignedInfo.ComputeHash(hash);
            if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance && (signatureMethod == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"))
            {
                formatter.SetHashAlgorithm("SHA256");
                buffer = formatter.CreateSignature(hash.Hash);
            }
            else
            {
                buffer = formatter.CreateSignature(hash);
            }
            this.Signature.SetSignatureValue(buffer);
        }

        public void EnsureDigestValidity(string id, object resolvedXmlSource)
        {
            this.Signature.SignedInfo.EnsureDigestValidity(id, resolvedXmlSource);
        }

        public byte[] GetSignatureValue()
        {
            return this.Signature.GetSignatureBytes();
        }

        public void ReadFrom(XmlDictionaryReader reader)
        {
            this.signature.ReadFrom(reader, this.dictionaryManager);
        }

        public void ReadFrom(XmlReader reader)
        {
            this.ReadFrom(XmlDictionaryReader.CreateDictionaryReader(reader));
        }

        public void StartSignatureVerification(SecurityKey verificationKey)
        {
            string signatureMethod = this.Signature.SignedInfo.SignatureMethod;
            SymmetricSecurityKey key = verificationKey as SymmetricSecurityKey;
            if (key != null)
            {
                using (KeyedHashAlgorithm algorithm = key.GetKeyedHashAlgorithm(signatureMethod))
                {
                    if (algorithm == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToCreateKeyedHashAlgorithmFromSymmetricCrypto", new object[] { signatureMethod, key })));
                    }
                    this.VerifySignature(algorithm);
                    return;
                }
            }
            AsymmetricSecurityKey key2 = verificationKey as AsymmetricSecurityKey;
            if (key2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("UnknownICryptoType", new object[] { verificationKey })));
            }
            using (HashAlgorithm algorithm2 = key2.GetHashAlgorithmForSignature(signatureMethod))
            {
                if (algorithm2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToCreateHashAlgorithmFromAsymmetricCrypto", new object[] { signatureMethod, key2 })));
                }
                AsymmetricSignatureDeformatter signatureDeformatter = key2.GetSignatureDeformatter(signatureMethod);
                if (signatureDeformatter == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("UnableToCreateSignatureDeformatterFromAsymmetricCrypto", new object[] { signatureMethod, key2 })));
                }
                this.VerifySignature(algorithm2, signatureDeformatter, signatureMethod);
            }
        }

        private void VerifySignature(KeyedHashAlgorithm hash)
        {
            this.Signature.SignedInfo.ComputeHash(hash);
            if (!CryptoHelper.IsEqual(hash.Hash, this.GetSignatureValue()))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("SignatureVerificationFailed")));
            }
        }

        private void VerifySignature(HashAlgorithm hash, AsymmetricSignatureDeformatter deformatter, string signatureMethod)
        {
            bool flag;
            this.Signature.SignedInfo.ComputeHash(hash);
            if (System.IdentityModel.SecurityUtils.RequiresFipsCompliance && (signatureMethod == "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"))
            {
                deformatter.SetHashAlgorithm("SHA256");
                flag = deformatter.VerifySignature(hash.Hash, this.GetSignatureValue());
            }
            else
            {
                flag = deformatter.VerifySignature(hash, this.GetSignatureValue());
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString("SignatureVerificationFailed")));
            }
        }

        public void WriteTo(XmlDictionaryWriter writer)
        {
            this.WriteTo(writer, this.dictionaryManager);
        }

        public void WriteTo(XmlDictionaryWriter writer, DictionaryManager dictionaryManager)
        {
            this.signature.WriteTo(writer, dictionaryManager);
        }

        public bool HasId
        {
            get
            {
                return true;
            }
        }

        public string Id
        {
            get
            {
                return this.signature.Id;
            }
            set
            {
                this.signature.Id = value;
            }
        }

        public System.IdentityModel.Selectors.SecurityTokenSerializer SecurityTokenSerializer
        {
            get
            {
                return this.tokenSerializer;
            }
        }

        public System.IdentityModel.Signature Signature
        {
            get
            {
                return this.signature;
            }
        }

        public System.IdentityModel.TransformFactory TransformFactory
        {
            get
            {
                return this.transformFactory;
            }
            set
            {
                this.transformFactory = value;
            }
        }
    }
}

