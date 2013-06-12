namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ECDiffieHellmanCng : ECDiffieHellman
    {
        private CngAlgorithm m_hashAlgorithm;
        private byte[] m_hmacKey;
        private ECDiffieHellmanKeyDerivationFunction m_kdf;
        private CngKey m_key;
        private byte[] m_label;
        private byte[] m_secretAppend;
        private byte[] m_secretPrepend;
        private byte[] m_seed;
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x100, 0x180, 0x80), new KeySizes(0x209, 0x209, 0) };

        public ECDiffieHellmanCng() : this(0x209)
        {
        }

        [SecurityCritical]
        public ECDiffieHellmanCng(int keySize)
        {
            this.m_hashAlgorithm = CngAlgorithm.Sha256;
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            base.LegalKeySizesValue = s_legalKeySizes;
            this.KeySize = keySize;
        }

        [SecurityCritical]
        public ECDiffieHellmanCng(CngKey key)
        {
            this.m_hashAlgorithm = CngAlgorithm.Sha256;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHRequiresECDHKey"), "key");
            }
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            base.LegalKeySizesValue = s_legalKeySizes;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            this.Key = CngKey.Open(key.Handle, key.IsEphemeral ? CngKeyHandleOpenOptions.EphemeralKey : CngKeyHandleOpenOptions.None);
            CodeAccessPermission.RevertAssert();
            this.KeySize = this.m_key.KeySize;
        }

        [SecurityCritical]
        public byte[] DeriveKeyMaterial(CngKey otherPartyPublicKey)
        {
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            if (otherPartyPublicKey.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHRequiresECDHKey"), "otherPartyPublicKey");
            }
            if (otherPartyPublicKey.KeySize != this.KeySize)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHKeySizeMismatch"), "otherPartyPublicKey");
            }
            NCryptNative.SecretAgreementFlags flags = this.UseSecretAgreementAsHmacKey ? NCryptNative.SecretAgreementFlags.UseSecretAsHmacKey : NCryptNative.SecretAgreementFlags.None;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            SafeNCryptKeyHandle privateKey = this.Key.Handle;
            SafeNCryptKeyHandle handle = otherPartyPublicKey.Handle;
            CodeAccessPermission.RevertAssert();
            using (SafeNCryptSecretHandle handle3 = NCryptNative.DeriveSecretAgreement(privateKey, handle))
            {
                if (this.KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hash)
                {
                    byte[] secretAppend = (this.SecretAppend == null) ? null : (this.SecretAppend.Clone() as byte[]);
                    byte[] secretPrepend = (this.SecretPrepend == null) ? null : (this.SecretPrepend.Clone() as byte[]);
                    return NCryptNative.DeriveKeyMaterialHash(handle3, this.HashAlgorithm.Algorithm, secretPrepend, secretAppend, flags);
                }
                if (this.KeyDerivationFunction == ECDiffieHellmanKeyDerivationFunction.Hmac)
                {
                    byte[] hmacKey = (this.HmacKey == null) ? null : (this.HmacKey.Clone() as byte[]);
                    byte[] buffer4 = (this.SecretAppend == null) ? null : (this.SecretAppend.Clone() as byte[]);
                    byte[] buffer5 = (this.SecretPrepend == null) ? null : (this.SecretPrepend.Clone() as byte[]);
                    return NCryptNative.DeriveKeyMaterialHmac(handle3, this.HashAlgorithm.Algorithm, hmacKey, buffer5, buffer4, flags);
                }
                byte[] label = (this.Label == null) ? null : (this.Label.Clone() as byte[]);
                byte[] seed = (this.Seed == null) ? null : (this.Seed.Clone() as byte[]);
                if ((label == null) || (seed == null))
                {
                    throw new InvalidOperationException(System.SR.GetString("Cryptography_TlsRequiresLabelAndSeed"));
                }
                return NCryptNative.DeriveKeyMaterialTls(handle3, label, seed, flags);
            }
        }

        public override byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey)
        {
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            ECDiffieHellmanCngPublicKey key = otherPartyPublicKey as ECDiffieHellmanCngPublicKey;
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgExpectedECDiffieHellmanCngPublicKey"));
            }
            using (CngKey key2 = key.Import())
            {
                return this.DeriveKeyMaterial(key2);
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public SafeNCryptSecretHandle DeriveSecretAgreementHandle(CngKey otherPartyPublicKey)
        {
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            if (otherPartyPublicKey.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHRequiresECDHKey"), "otherPartyPublicKey");
            }
            if (otherPartyPublicKey.KeySize != this.KeySize)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHKeySizeMismatch"), "otherPartyPublicKey");
            }
            return NCryptNative.DeriveSecretAgreement(this.Key.Handle, otherPartyPublicKey.Handle);
        }

        public SafeNCryptSecretHandle DeriveSecretAgreementHandle(ECDiffieHellmanPublicKey otherPartyPublicKey)
        {
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentNullException("otherPartyPublicKey");
            }
            ECDiffieHellmanCngPublicKey key = otherPartyPublicKey as ECDiffieHellmanCngPublicKey;
            if (otherPartyPublicKey == null)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgExpectedECDiffieHellmanCngPublicKey"));
            }
            using (CngKey key2 = key.Import())
            {
                return this.DeriveSecretAgreementHandle(key2);
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.m_key != null))
                {
                    this.m_key.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void FromXmlString(string xmlString)
        {
            throw new NotImplementedException(System.SR.GetString("Cryptography_ECXmlSerializationFormatRequired"));
        }

        public void FromXmlString(string xml, ECKeyXmlFormat format)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            if (format != ECKeyXmlFormat.Rfc4050)
            {
                throw new ArgumentOutOfRangeException("format");
            }
            this.Key = Rfc4050KeyFormatter.FromXml(xml);
        }

        public override string ToXmlString(bool includePrivateParameters)
        {
            throw new NotImplementedException(System.SR.GetString("Cryptography_ECXmlSerializationFormatRequired"));
        }

        public string ToXmlString(ECKeyXmlFormat format)
        {
            if (format != ECKeyXmlFormat.Rfc4050)
            {
                throw new ArgumentOutOfRangeException("format");
            }
            return Rfc4050KeyFormatter.ToXml(this.Key);
        }

        public CngAlgorithm HashAlgorithm
        {
            get
            {
                return this.m_hashAlgorithm;
            }
            set
            {
                if (this.m_hashAlgorithm == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_hashAlgorithm = value;
            }
        }

        public byte[] HmacKey
        {
            get
            {
                return this.m_hmacKey;
            }
            set
            {
                this.m_hmacKey = value;
            }
        }

        public CngKey Key
        {
            get
            {
                if ((this.m_key != null) && (this.m_key.KeySize != this.KeySize))
                {
                    this.m_key.Dispose();
                    this.m_key = null;
                }
                if (this.m_key == null)
                {
                    CngAlgorithm algorithm = null;
                    switch (this.KeySize)
                    {
                        case 0x100:
                            algorithm = CngAlgorithm.ECDiffieHellmanP256;
                            break;

                        case 0x180:
                            algorithm = CngAlgorithm.ECDiffieHellmanP384;
                            break;

                        case 0x209:
                            algorithm = CngAlgorithm.ECDiffieHellmanP521;
                            break;
                    }
                    this.m_key = CngKey.Create(algorithm);
                }
                return this.m_key;
            }
            private set
            {
                if (value.AlgorithmGroup != CngAlgorithmGroup.ECDiffieHellman)
                {
                    throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDHRequiresECDHKey"));
                }
                if (this.m_key != null)
                {
                    this.m_key.Dispose();
                }
                this.m_key = value;
                this.KeySize = this.m_key.KeySize;
            }
        }

        public ECDiffieHellmanKeyDerivationFunction KeyDerivationFunction
        {
            get
            {
                return this.m_kdf;
            }
            set
            {
                if ((value < ECDiffieHellmanKeyDerivationFunction.Hash) || (value > ECDiffieHellmanKeyDerivationFunction.Tls))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_kdf = value;
            }
        }

        public byte[] Label
        {
            get
            {
                return this.m_label;
            }
            set
            {
                this.m_label = value;
            }
        }

        public override ECDiffieHellmanPublicKey PublicKey
        {
            [SecurityCritical]
            get
            {
                return new ECDiffieHellmanCngPublicKey(this.Key);
            }
        }

        public byte[] SecretAppend
        {
            get
            {
                return this.m_secretAppend;
            }
            set
            {
                this.m_secretAppend = value;
            }
        }

        public byte[] SecretPrepend
        {
            get
            {
                return this.m_secretPrepend;
            }
            set
            {
                this.m_secretPrepend = value;
            }
        }

        public byte[] Seed
        {
            get
            {
                return this.m_seed;
            }
            set
            {
                this.m_seed = value;
            }
        }

        public bool UseSecretAgreementAsHmacKey
        {
            get
            {
                return (this.HmacKey == null);
            }
        }
    }
}

