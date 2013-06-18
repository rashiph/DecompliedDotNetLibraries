namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Security.Tokens;
    using System.Xml;

    [TypeConverter(typeof(SecurityAlgorithmSuiteConverter))]
    public abstract class SecurityAlgorithmSuite
    {
        private static SecurityAlgorithmSuite basic128;
        private static SecurityAlgorithmSuite basic128Rsa15;
        private static SecurityAlgorithmSuite basic128Sha256;
        private static SecurityAlgorithmSuite basic128Sha256Rsa15;
        private static SecurityAlgorithmSuite basic192;
        private static SecurityAlgorithmSuite basic192Rsa15;
        private static SecurityAlgorithmSuite basic192Sha256;
        private static SecurityAlgorithmSuite basic192Sha256Rsa15;
        private static SecurityAlgorithmSuite basic256;
        private static SecurityAlgorithmSuite basic256Rsa15;
        private static SecurityAlgorithmSuite basic256Sha256;
        private static SecurityAlgorithmSuite basic256Sha256Rsa15;
        private static SecurityAlgorithmSuite tripleDes;
        private static SecurityAlgorithmSuite tripleDesRsa15;
        private static SecurityAlgorithmSuite tripleDesSha256;
        private static SecurityAlgorithmSuite tripleDesSha256Rsa15;

        protected SecurityAlgorithmSuite()
        {
        }

        internal void EnsureAcceptableAsymmetricSignatureAlgorithm(string algorithm)
        {
            if (!this.IsAsymmetricSignatureAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "AsymmetricSignature", this })));
            }
        }

        internal void EnsureAcceptableDecryptionSymmetricKeySize(SymmetricSecurityKey securityKey, SecurityToken token)
        {
            int keySize;
            DerivedKeySecurityToken token2 = token as DerivedKeySecurityToken;
            if (token2 != null)
            {
                token = token2.TokenToDerive;
                keySize = ((SymmetricSecurityKey) token.SecurityKeys[0]).KeySize;
                if (token2.SecurityKeys[0].KeySize < this.DefaultEncryptionKeyDerivationLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenDoesNotMeetKeySizeRequirements", new object[] { this, token2, token2.SecurityKeys[0].KeySize })));
                }
            }
            else
            {
                keySize = securityKey.KeySize;
            }
            if (!this.IsSymmetricKeyLengthSupported(keySize))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenDoesNotMeetKeySizeRequirements", new object[] { this, token, keySize })));
            }
        }

        internal void EnsureAcceptableDigestAlgorithm(string algorithm)
        {
            if (!this.IsDigestAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "Digest", this })));
            }
        }

        internal void EnsureAcceptableEncryptionAlgorithm(string algorithm)
        {
            if (!this.IsEncryptionAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "Encryption", this })));
            }
        }

        internal void EnsureAcceptableEncryptionKeyDerivationAlgorithm(string algorithm)
        {
            if (!this.IsEncryptionKeyDerivationAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "EncryptionKeyDerivation", this })));
            }
        }

        internal void EnsureAcceptableKeyWrapAlgorithm(string algorithm, bool isAsymmetric)
        {
            if (isAsymmetric)
            {
                if (!this.IsAsymmetricKeyWrapAlgorithmSupported(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "AsymmetricKeyWrap", this })));
                }
            }
            else if (!this.IsSymmetricKeyWrapAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "SymmetricKeyWrap", this })));
            }
        }

        internal void EnsureAcceptableSignatureAlgorithm(SecurityKey verificationKey, string algorithm)
        {
            if (verificationKey is InMemorySymmetricSecurityKey)
            {
                this.EnsureAcceptableSymmetricSignatureAlgorithm(algorithm);
            }
            else
            {
                if (!(verificationKey is AsymmetricSecurityKey))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnknownICryptoType", new object[] { verificationKey })));
                }
                this.EnsureAcceptableAsymmetricSignatureAlgorithm(algorithm);
            }
        }

        internal void EnsureAcceptableSignatureKeyDerivationAlgorithm(string algorithm)
        {
            if (!this.IsSignatureKeyDerivationAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "SignatureKeyDerivation", this })));
            }
        }

        internal void EnsureAcceptableSignatureKeySize(SecurityKey securityKey, SecurityToken token)
        {
            AsymmetricSecurityKey key = securityKey as AsymmetricSecurityKey;
            if (key != null)
            {
                if (!this.IsAsymmetricKeyLengthSupported(key.KeySize))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenDoesNotMeetKeySizeRequirements", new object[] { this, token, key.KeySize })));
                }
            }
            else
            {
                SymmetricSecurityKey key2 = securityKey as SymmetricSecurityKey;
                if (key2 == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("UnknownICryptoType", new object[] { key2 })));
                }
                this.EnsureAcceptableSignatureSymmetricKeySize(key2, token);
            }
        }

        internal void EnsureAcceptableSignatureSymmetricKeySize(SymmetricSecurityKey securityKey, SecurityToken token)
        {
            int keySize;
            DerivedKeySecurityToken token2 = token as DerivedKeySecurityToken;
            if (token2 != null)
            {
                token = token2.TokenToDerive;
                keySize = ((SymmetricSecurityKey) token.SecurityKeys[0]).KeySize;
                if (token2.SecurityKeys[0].KeySize < this.DefaultSignatureKeyDerivationLength)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenDoesNotMeetKeySizeRequirements", new object[] { this, token2, token2.SecurityKeys[0].KeySize })));
                }
            }
            else
            {
                keySize = securityKey.KeySize;
            }
            if (!this.IsSymmetricKeyLengthSupported(keySize))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("TokenDoesNotMeetKeySizeRequirements", new object[] { this, token, keySize })));
            }
        }

        internal void EnsureAcceptableSymmetricSignatureAlgorithm(string algorithm)
        {
            if (!this.IsSymmetricSignatureAlgorithmSupported(algorithm))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessageSecurityException(System.ServiceModel.SR.GetString("SuiteDoesNotAcceptAlgorithm", new object[] { algorithm, "SymmetricSignature", this })));
            }
        }

        internal string GetEncryptionKeyDerivationAlgorithm(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(keyDerivationAlgorithm, token))
            {
                return keyDerivationAlgorithm;
            }
            return null;
        }

        internal int GetEncryptionKeyDerivationLength(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (!System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(version), token))
            {
                return 0;
            }
            if ((this.DefaultEncryptionKeyDerivationLength % 8) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("Psha1KeyLengthInvalid", new object[] { this.DefaultEncryptionKeyDerivationLength })));
            }
            return (this.DefaultEncryptionKeyDerivationLength / 8);
        }

        internal void GetKeyWrapAlgorithm(SecurityToken token, out string keyWrapAlgorithm, out XmlDictionaryString keyWrapAlgorithmDictionaryString)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(this.DefaultSymmetricKeyWrapAlgorithm, token))
            {
                keyWrapAlgorithm = this.DefaultSymmetricKeyWrapAlgorithm;
                keyWrapAlgorithmDictionaryString = this.DefaultSymmetricKeyWrapAlgorithmDictionaryString;
            }
            else
            {
                keyWrapAlgorithm = this.DefaultAsymmetricKeyWrapAlgorithm;
                keyWrapAlgorithmDictionaryString = this.DefaultAsymmetricKeyWrapAlgorithmDictionaryString;
            }
        }

        internal void GetSignatureAlgorithmAndKey(SecurityToken token, out string signatureAlgorithm, out SecurityKey key, out XmlDictionaryString signatureAlgorithmDictionaryString)
        {
            ReadOnlyCollection<SecurityKey> securityKeys = token.SecurityKeys;
            if ((securityKeys == null) || (securityKeys.Count == 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SigningTokenHasNoKeys", new object[] { token })));
            }
            for (int i = 0; i < securityKeys.Count; i++)
            {
                if (securityKeys[i].IsSupportedAlgorithm(this.DefaultSymmetricSignatureAlgorithm))
                {
                    signatureAlgorithm = this.DefaultSymmetricSignatureAlgorithm;
                    signatureAlgorithmDictionaryString = this.DefaultSymmetricSignatureAlgorithmDictionaryString;
                    key = securityKeys[i];
                    return;
                }
                if (securityKeys[i].IsSupportedAlgorithm(this.DefaultAsymmetricSignatureAlgorithm))
                {
                    signatureAlgorithm = this.DefaultAsymmetricSignatureAlgorithm;
                    signatureAlgorithmDictionaryString = this.DefaultAsymmetricSignatureAlgorithmDictionaryString;
                    key = securityKeys[i];
                    return;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SigningTokenHasNoKeysSupportingTheAlgorithmSuite", new object[] { token, this })));
        }

        internal string GetSignatureKeyDerivationAlgorithm(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            string keyDerivationAlgorithm = System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(version);
            if (System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(keyDerivationAlgorithm, token))
            {
                return keyDerivationAlgorithm;
            }
            return null;
        }

        internal int GetSignatureKeyDerivationLength(SecurityToken token, SecureConversationVersion version)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
            if (!System.ServiceModel.Security.SecurityUtils.IsSupportedAlgorithm(System.ServiceModel.Security.SecurityUtils.GetKeyDerivationAlgorithm(version), token))
            {
                return 0;
            }
            if ((this.DefaultSignatureKeyDerivationLength % 8) != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("Psha1KeyLengthInvalid", new object[] { this.DefaultSignatureKeyDerivationLength })));
            }
            return (this.DefaultSignatureKeyDerivationLength / 8);
        }

        public abstract bool IsAsymmetricKeyLengthSupported(int length);
        public virtual bool IsAsymmetricKeyWrapAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultAsymmetricKeyWrapAlgorithm);
        }

        public virtual bool IsAsymmetricSignatureAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultAsymmetricSignatureAlgorithm);
        }

        public virtual bool IsCanonicalizationAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultCanonicalizationAlgorithm);
        }

        public virtual bool IsDigestAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultDigestAlgorithm);
        }

        public virtual bool IsEncryptionAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultEncryptionAlgorithm);
        }

        public virtual bool IsEncryptionKeyDerivationAlgorithmSupported(string algorithm)
        {
            if (!(algorithm == "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1"))
            {
                return (algorithm == "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1");
            }
            return true;
        }

        internal static bool IsRsaSHA256(SecurityAlgorithmSuite suite)
        {
            if (suite == null)
            {
                return false;
            }
            if ((((suite != Basic128Sha256) && (suite != Basic128Sha256Rsa15)) && ((suite != Basic192Sha256) && (suite != Basic192Sha256Rsa15))) && (((suite != Basic256Sha256) && (suite != Basic256Sha256Rsa15)) && (suite != TripleDesSha256)))
            {
                return (suite == TripleDesSha256Rsa15);
            }
            return true;
        }

        public virtual bool IsSignatureKeyDerivationAlgorithmSupported(string algorithm)
        {
            if (!(algorithm == "http://schemas.xmlsoap.org/ws/2005/02/sc/dk/p_sha1"))
            {
                return (algorithm == "http://docs.oasis-open.org/ws-sx/ws-secureconversation/200512/dk/p_sha1");
            }
            return true;
        }

        public abstract bool IsSymmetricKeyLengthSupported(int length);
        public virtual bool IsSymmetricKeyWrapAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultSymmetricKeyWrapAlgorithm);
        }

        public virtual bool IsSymmetricSignatureAlgorithmSupported(string algorithm)
        {
            return (algorithm == this.DefaultSymmetricSignatureAlgorithm);
        }

        public static SecurityAlgorithmSuite Basic128
        {
            get
            {
                if (basic128 == null)
                {
                    basic128 = new Basic128SecurityAlgorithmSuite();
                }
                return basic128;
            }
        }

        public static SecurityAlgorithmSuite Basic128Rsa15
        {
            get
            {
                if (basic128Rsa15 == null)
                {
                    basic128Rsa15 = new Basic128Rsa15SecurityAlgorithmSuite();
                }
                return basic128Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Basic128Sha256
        {
            get
            {
                if (basic128Sha256 == null)
                {
                    basic128Sha256 = new Basic128Sha256SecurityAlgorithmSuite();
                }
                return basic128Sha256;
            }
        }

        public static SecurityAlgorithmSuite Basic128Sha256Rsa15
        {
            get
            {
                if (basic128Sha256Rsa15 == null)
                {
                    basic128Sha256Rsa15 = new Basic128Sha256Rsa15SecurityAlgorithmSuite();
                }
                return basic128Sha256Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Basic192
        {
            get
            {
                if (basic192 == null)
                {
                    basic192 = new Basic192SecurityAlgorithmSuite();
                }
                return basic192;
            }
        }

        public static SecurityAlgorithmSuite Basic192Rsa15
        {
            get
            {
                if (basic192Rsa15 == null)
                {
                    basic192Rsa15 = new Basic192Rsa15SecurityAlgorithmSuite();
                }
                return basic192Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Basic192Sha256
        {
            get
            {
                if (basic192Sha256 == null)
                {
                    basic192Sha256 = new Basic192Sha256SecurityAlgorithmSuite();
                }
                return basic192Sha256;
            }
        }

        public static SecurityAlgorithmSuite Basic192Sha256Rsa15
        {
            get
            {
                if (basic192Sha256Rsa15 == null)
                {
                    basic192Sha256Rsa15 = new Basic192Sha256Rsa15SecurityAlgorithmSuite();
                }
                return basic192Sha256Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Basic256
        {
            get
            {
                if (basic256 == null)
                {
                    basic256 = new Basic256SecurityAlgorithmSuite();
                }
                return basic256;
            }
        }

        public static SecurityAlgorithmSuite Basic256Rsa15
        {
            get
            {
                if (basic256Rsa15 == null)
                {
                    basic256Rsa15 = new Basic256Rsa15SecurityAlgorithmSuite();
                }
                return basic256Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Basic256Sha256
        {
            get
            {
                if (basic256Sha256 == null)
                {
                    basic256Sha256 = new Basic256Sha256SecurityAlgorithmSuite();
                }
                return basic256Sha256;
            }
        }

        public static SecurityAlgorithmSuite Basic256Sha256Rsa15
        {
            get
            {
                if (basic256Sha256Rsa15 == null)
                {
                    basic256Sha256Rsa15 = new Basic256Sha256Rsa15SecurityAlgorithmSuite();
                }
                return basic256Sha256Rsa15;
            }
        }

        public static SecurityAlgorithmSuite Default
        {
            get
            {
                return Basic256;
            }
        }

        public abstract string DefaultAsymmetricKeyWrapAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultAsymmetricKeyWrapAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract string DefaultAsymmetricSignatureAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultAsymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract string DefaultCanonicalizationAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultCanonicalizationAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract string DefaultDigestAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultDigestAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract string DefaultEncryptionAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultEncryptionAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract int DefaultEncryptionKeyDerivationLength { get; }

        public abstract int DefaultSignatureKeyDerivationLength { get; }

        public abstract int DefaultSymmetricKeyLength { get; }

        public abstract string DefaultSymmetricKeyWrapAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultSymmetricKeyWrapAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        public abstract string DefaultSymmetricSignatureAlgorithm { get; }

        internal virtual XmlDictionaryString DefaultSymmetricSignatureAlgorithmDictionaryString
        {
            get
            {
                return null;
            }
        }

        internal static SecurityAlgorithmSuite KerberosDefault
        {
            get
            {
                return Basic128;
            }
        }

        public static SecurityAlgorithmSuite TripleDes
        {
            get
            {
                if (tripleDes == null)
                {
                    tripleDes = new TripleDesSecurityAlgorithmSuite();
                }
                return tripleDes;
            }
        }

        public static SecurityAlgorithmSuite TripleDesRsa15
        {
            get
            {
                if (tripleDesRsa15 == null)
                {
                    tripleDesRsa15 = new TripleDesRsa15SecurityAlgorithmSuite();
                }
                return tripleDesRsa15;
            }
        }

        public static SecurityAlgorithmSuite TripleDesSha256
        {
            get
            {
                if (tripleDesSha256 == null)
                {
                    tripleDesSha256 = new TripleDesSha256SecurityAlgorithmSuite();
                }
                return tripleDesSha256;
            }
        }

        public static SecurityAlgorithmSuite TripleDesSha256Rsa15
        {
            get
            {
                if (tripleDesSha256Rsa15 == null)
                {
                    tripleDesSha256Rsa15 = new TripleDesSha256Rsa15SecurityAlgorithmSuite();
                }
                return tripleDesSha256Rsa15;
            }
        }
    }
}

