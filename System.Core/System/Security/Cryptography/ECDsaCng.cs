namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ECDsaCng : ECDsa
    {
        private CngAlgorithm m_hashAlgorithm;
        private CngKey m_key;
        private static KeySizes[] s_legalKeySizes = new KeySizes[] { new KeySizes(0x100, 0x180, 0x80), new KeySizes(0x209, 0x209, 0) };

        public ECDsaCng() : this(0x209)
        {
        }

        [SecurityCritical]
        public ECDsaCng(int keySize)
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
        public ECDsaCng(CngKey key)
        {
            this.m_hashAlgorithm = CngAlgorithm.Sha256;
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (key.AlgorithmGroup != CngAlgorithmGroup.ECDsa)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDsaRequiresECDsaKey"), "key");
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

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (this.m_key != null)
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

        public byte[] SignData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return this.SignData(data, 0, data.Length);
        }

        [SecurityCritical]
        public byte[] SignData(Stream data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            using (BCryptHashAlgorithm algorithm = new BCryptHashAlgorithm(this.HashAlgorithm, "Microsoft Primitive Provider"))
            {
                algorithm.HashStream(data);
                byte[] hash = algorithm.HashFinal();
                return this.SignHash(hash);
            }
        }

        [SecurityCritical]
        public byte[] SignData(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) || (offset > data.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > (data.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            using (BCryptHashAlgorithm algorithm = new BCryptHashAlgorithm(this.HashAlgorithm, "Microsoft Primitive Provider"))
            {
                algorithm.HashCore(data, offset, count);
                byte[] hash = algorithm.HashFinal();
                return this.SignHash(hash);
            }
        }

        [SecurityCritical]
        public override byte[] SignHash(byte[] hash)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }
            KeyContainerPermission permission = this.Key.BuildKeyContainerPermission(KeyContainerPermissionFlags.Sign);
            if (permission != null)
            {
                permission.Demand();
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            SafeNCryptKeyHandle key = this.Key.Handle;
            CodeAccessPermission.RevertAssert();
            return NCryptNative.SignHash(key, hash);
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

        public bool VerifyData(byte[] data, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            return this.VerifyData(data, 0, data.Length, signature);
        }

        [SecurityCritical]
        public bool VerifyData(Stream data, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }
            using (BCryptHashAlgorithm algorithm = new BCryptHashAlgorithm(this.HashAlgorithm, "Microsoft Primitive Provider"))
            {
                algorithm.HashStream(data);
                byte[] hash = algorithm.HashFinal();
                return this.VerifyHash(hash, signature);
            }
        }

        [SecurityCritical]
        public bool VerifyData(byte[] data, int offset, int count, byte[] signature)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if ((offset < 0) || (offset > data.Length))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((count < 0) || (count > (data.Length - offset)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }
            using (BCryptHashAlgorithm algorithm = new BCryptHashAlgorithm(this.HashAlgorithm, "Microsoft Primitive Provider"))
            {
                algorithm.HashCore(data, offset, count);
                byte[] hash = algorithm.HashFinal();
                return this.VerifyHash(hash, signature);
            }
        }

        [SecurityCritical]
        public override bool VerifyHash(byte[] hash, byte[] signature)
        {
            if (hash == null)
            {
                throw new ArgumentNullException("hash");
            }
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            SafeNCryptKeyHandle key = this.Key.Handle;
            CodeAccessPermission.RevertAssert();
            return NCryptNative.VerifySignature(key, hash, signature);
        }

        public CngAlgorithm HashAlgorithm
        {
            get
            {
                return this.m_hashAlgorithm;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_hashAlgorithm = value;
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
                            algorithm = CngAlgorithm.ECDsaP256;
                            break;

                        case 0x180:
                            algorithm = CngAlgorithm.ECDsaP384;
                            break;

                        case 0x209:
                            algorithm = CngAlgorithm.ECDsaP521;
                            break;
                    }
                    this.m_key = CngKey.Create(algorithm);
                }
                return this.m_key;
            }
            private set
            {
                if (value.AlgorithmGroup != CngAlgorithmGroup.ECDsa)
                {
                    throw new ArgumentException(System.SR.GetString("Cryptography_ArgECDsaRequiresECDsaKey"));
                }
                if (this.m_key != null)
                {
                    this.m_key.Dispose();
                }
                this.m_key = value;
                this.KeySize = this.m_key.KeySize;
            }
        }
    }
}

