namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class AesCryptoServiceProvider : Aes
    {
        private SafeCspHandle m_cspHandle;
        private SafeCapiKeyHandle m_key;
        private static int s_defaultKeySize;
        private static KeySizes[] s_supportedKeySizes;

        [SecurityCritical]
        public AesCryptoServiceProvider()
        {
            string providerName = "Microsoft Enhanced RSA and AES Cryptographic Provider";
            if ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 1))
            {
                providerName = "Microsoft Enhanced RSA and AES Cryptographic Provider (Prototype)";
            }
            this.m_cspHandle = CapiNative.AcquireCsp(null, providerName, CapiNative.ProviderType.RsaAes, CapiNative.CryptAcquireContextFlags.None | CapiNative.CryptAcquireContextFlags.VerifyContext, true);
            base.FeedbackSizeValue = 8;
            int defaultKeySize = 0;
            if (FindSupportedKeySizes(this.m_cspHandle, out defaultKeySize).Length == 0)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            base.KeySizeValue = defaultKeySize;
        }

        [SecurityCritical]
        public override ICryptoTransform CreateDecryptor()
        {
            if (((this.m_key == null) || this.m_key.IsInvalid) || this.m_key.IsClosed)
            {
                throw new CryptographicException(System.SR.GetString("Cryptography_DecryptWithNoKey"));
            }
            return this.CreateDecryptor(this.m_key, base.IVValue);
        }

        [SecurityCritical]
        public override ICryptoTransform CreateDecryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidKeySize"), "key");
            }
            if ((iv != null) && ((iv.Length * 8) != base.BlockSizeValue))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidIVSize"), "iv");
            }
            byte[] buffer = (byte[]) key.Clone();
            byte[] buffer2 = null;
            if (iv != null)
            {
                buffer2 = (byte[]) iv.Clone();
            }
            using (SafeCapiKeyHandle handle = CapiNative.ImportSymmetricKey(this.m_cspHandle, GetAlgorithmId(buffer.Length * 8), buffer))
            {
                return this.CreateDecryptor(handle, buffer2);
            }
        }

        [SecurityCritical]
        private ICryptoTransform CreateDecryptor(SafeCapiKeyHandle key, byte[] iv)
        {
            return new CapiSymmetricAlgorithm(base.BlockSizeValue, base.FeedbackSizeValue, this.m_cspHandle, key, iv, this.Mode, base.PaddingValue, EncryptionMode.Decrypt);
        }

        [SecurityCritical]
        public override ICryptoTransform CreateEncryptor()
        {
            if (((this.m_key == null) || this.m_key.IsInvalid) || this.m_key.IsClosed)
            {
                this.GenerateKey();
            }
            if ((this.Mode != CipherMode.ECB) && (base.IVValue == null))
            {
                this.GenerateIV();
            }
            return this.CreateEncryptor(this.m_key, base.IVValue);
        }

        [SecurityCritical]
        public override ICryptoTransform CreateEncryptor(byte[] key, byte[] iv)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (!base.ValidKeySize(key.Length * 8))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidKeySize"), "key");
            }
            if ((iv != null) && ((iv.Length * 8) != base.BlockSizeValue))
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_InvalidIVSize"), "iv");
            }
            byte[] buffer = (byte[]) key.Clone();
            byte[] buffer2 = null;
            if (iv != null)
            {
                buffer2 = (byte[]) iv.Clone();
            }
            using (SafeCapiKeyHandle handle = CapiNative.ImportSymmetricKey(this.m_cspHandle, GetAlgorithmId(buffer.Length * 8), buffer))
            {
                return this.CreateEncryptor(handle, buffer2);
            }
        }

        [SecurityCritical]
        private ICryptoTransform CreateEncryptor(SafeCapiKeyHandle key, byte[] iv)
        {
            return new CapiSymmetricAlgorithm(base.BlockSizeValue, base.FeedbackSizeValue, this.m_cspHandle, key, iv, this.Mode, base.PaddingValue, EncryptionMode.Encrypt);
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (this.m_key != null)
                    {
                        this.m_key.Dispose();
                    }
                    if (this.m_cspHandle != null)
                    {
                        this.m_cspHandle.Dispose();
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        [SecurityCritical]
        private static KeySizes[] FindSupportedKeySizes(SafeCspHandle csp, out int defaultKeySize)
        {
            if (s_supportedKeySizes == null)
            {
                List<KeySizes> list = new List<KeySizes>();
                int num = 0;
                for (CapiNative.PROV_ENUMALGS prov_enumalgs = CapiNative.GetProviderParameterStruct<CapiNative.PROV_ENUMALGS>(csp, CapiNative.ProviderParameter.EnumerateAlgorithms, CapiNative.ProviderParameterFlags.RestartEnumeration); prov_enumalgs.aiAlgId != CapiNative.AlgorithmId.None; prov_enumalgs = CapiNative.GetProviderParameterStruct<CapiNative.PROV_ENUMALGS>(csp, CapiNative.ProviderParameter.EnumerateAlgorithms, CapiNative.ProviderParameterFlags.None))
                {
                    switch (prov_enumalgs.aiAlgId)
                    {
                        case CapiNative.AlgorithmId.Aes128:
                            list.Add(new KeySizes(0x80, 0x80, 0));
                            if (0x80 > num)
                            {
                                num = 0x80;
                            }
                            break;

                        case CapiNative.AlgorithmId.Aes192:
                            list.Add(new KeySizes(0xc0, 0xc0, 0));
                            if (0xc0 > num)
                            {
                                num = 0xc0;
                            }
                            break;

                        case CapiNative.AlgorithmId.Aes256:
                            list.Add(new KeySizes(0x100, 0x100, 0));
                            if (0x100 > num)
                            {
                                num = 0x100;
                            }
                            break;
                    }
                }
                s_supportedKeySizes = list.ToArray();
                s_defaultKeySize = num;
            }
            defaultKeySize = s_defaultKeySize;
            return s_supportedKeySizes;
        }

        [SecurityCritical]
        public override void GenerateIV()
        {
            byte[] pbBuffer = new byte[base.BlockSizeValue / 8];
            if (!CapiNative.UnsafeNativeMethods.CryptGenRandom(this.m_cspHandle, pbBuffer.Length, pbBuffer))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            base.IVValue = pbBuffer;
        }

        [SecurityCritical]
        public override void GenerateKey()
        {
            SafeCapiKeyHandle phKey = null;
            if (!CapiNative.UnsafeNativeMethods.CryptGenKey(this.m_cspHandle, GetAlgorithmId(base.KeySizeValue), CapiNative.KeyFlags.Exportable, out phKey))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            if (this.m_key != null)
            {
                this.m_key.Dispose();
            }
            this.m_key = phKey;
        }

        private static CapiNative.AlgorithmId GetAlgorithmId(int keySize)
        {
            switch (keySize)
            {
                case 0x80:
                    return CapiNative.AlgorithmId.Aes128;

                case 0xc0:
                    return CapiNative.AlgorithmId.Aes192;

                case 0x100:
                    return CapiNative.AlgorithmId.Aes256;
            }
            return CapiNative.AlgorithmId.None;
        }

        public override byte[] Key
        {
            [SecurityCritical]
            get
            {
                if (((this.m_key == null) || this.m_key.IsInvalid) || this.m_key.IsClosed)
                {
                    this.GenerateKey();
                }
                return CapiNative.ExportSymmetricKey(this.m_key);
            }
            [SecurityCritical]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                byte[] key = (byte[]) value.Clone();
                if (!base.ValidKeySize(key.Length * 8))
                {
                    throw new CryptographicException(System.SR.GetString("Cryptography_InvalidKeySize"));
                }
                SafeCapiKeyHandle handle = CapiNative.ImportSymmetricKey(this.m_cspHandle, GetAlgorithmId(key.Length * 8), key);
                if (this.m_key != null)
                {
                    this.m_key.Dispose();
                }
                this.m_key = handle;
                base.KeySizeValue = key.Length * 8;
            }
        }

        public override int KeySize
        {
            get
            {
                return base.KeySize;
            }
            [SecurityCritical]
            set
            {
                base.KeySize = value;
                if (this.m_key != null)
                {
                    this.m_key.Dispose();
                }
            }
        }
    }
}

