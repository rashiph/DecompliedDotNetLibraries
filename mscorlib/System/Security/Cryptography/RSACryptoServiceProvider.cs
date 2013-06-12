namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [ComVisible(true)]
    public sealed class RSACryptoServiceProvider : RSA, ICspAsymmetricAlgorithm
    {
        private int _dwKeySize;
        private CspParameters _parameters;
        private bool _randomKeyContainer;
        [SecurityCritical]
        private SafeKeyHandle _safeKeyHandle;
        [SecurityCritical]
        private SafeProvHandle _safeProvHandle;
        private static CspProviderFlags s_UseMachineKeyStore;

        [SecuritySafeCritical]
        public RSACryptoServiceProvider() : this(0, new CspParameters(Utils.DefaultRsaProviderType, null, null, s_UseMachineKeyStore), true)
        {
        }

        [SecuritySafeCritical]
        public RSACryptoServiceProvider(int dwKeySize) : this(dwKeySize, new CspParameters(Utils.DefaultRsaProviderType, null, null, s_UseMachineKeyStore), false)
        {
        }

        [SecuritySafeCritical]
        public RSACryptoServiceProvider(CspParameters parameters) : this(0, parameters, true)
        {
        }

        [SecuritySafeCritical]
        public RSACryptoServiceProvider(int dwKeySize, CspParameters parameters) : this(dwKeySize, parameters, false)
        {
        }

        [SecurityCritical]
        private RSACryptoServiceProvider(int dwKeySize, CspParameters parameters, bool useDefaultKeySize)
        {
            if (dwKeySize < 0)
            {
                throw new ArgumentOutOfRangeException("dwKeySize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this._parameters = Utils.SaveCspParameters(CspAlgorithmType.Rsa, parameters, s_UseMachineKeyStore, ref this._randomKeyContainer);
            base.LegalKeySizesValue = new KeySizes[] { new KeySizes(0x180, 0x4000, 8) };
            this._dwKeySize = useDefaultKeySize ? 0x400 : dwKeySize;
            if (!this._randomKeyContainer || Environment.GetCompatibilityFlag(CompatibilityFlag.EagerlyGenerateRandomAsymmKeys))
            {
                this.GetKeyPair();
            }
        }

        [SecuritySafeCritical]
        public byte[] Decrypt(byte[] rgb, bool fOAEP)
        {
            if (rgb == null)
            {
                throw new ArgumentNullException("rgb");
            }
            this.GetKeyPair();
            if (rgb.Length > (this.KeySize / 8))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_Padding_DecDataTooBig", new object[] { this.KeySize / 8 }));
            }
            if (!this.CspKeyContainerInfo.RandomlyGenerated)
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Decrypt);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            byte[] o = null;
            DecryptKey(this._safeKeyHandle, rgb, rgb.Length, fOAEP, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            return o;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DecryptKey(SafeKeyHandle pKeyContext, [MarshalAs(UnmanagedType.LPArray)] byte[] pbEncryptedKey, int cbEncryptedKey, [MarshalAs(UnmanagedType.Bool)] bool fOAEP, ObjectHandleOnStack ohRetDecryptedKey);
        public override byte[] DecryptValue(byte[] rgb)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if ((this._safeKeyHandle != null) && !this._safeKeyHandle.IsClosed)
            {
                this._safeKeyHandle.Dispose();
            }
            if ((this._safeProvHandle != null) && !this._safeProvHandle.IsClosed)
            {
                this._safeProvHandle.Dispose();
            }
        }

        [SecuritySafeCritical]
        public byte[] Encrypt(byte[] rgb, bool fOAEP)
        {
            if (rgb == null)
            {
                throw new ArgumentNullException("rgb");
            }
            this.GetKeyPair();
            byte[] o = null;
            EncryptKey(this._safeKeyHandle, rgb, rgb.Length, fOAEP, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void EncryptKey(SafeKeyHandle pKeyContext, [MarshalAs(UnmanagedType.LPArray)] byte[] pbKey, int cbKey, [MarshalAs(UnmanagedType.Bool)] bool fOAEP, ObjectHandleOnStack ohRetEncryptedKey);
        public override byte[] EncryptValue(byte[] rgb)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_Method"));
        }

        [SecuritySafeCritical, ComVisible(false)]
        public byte[] ExportCspBlob(bool includePrivateParameters)
        {
            this.GetKeyPair();
            return Utils.ExportCspBlobHelper(includePrivateParameters, this._parameters, this._safeKeyHandle);
        }

        [SecuritySafeCritical]
        public override RSAParameters ExportParameters(bool includePrivateParameters)
        {
            this.GetKeyPair();
            if (includePrivateParameters)
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Export);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            RSACspObject cspObject = new RSACspObject();
            int blobType = includePrivateParameters ? 7 : 6;
            Utils._ExportKey(this._safeKeyHandle, blobType, cspObject);
            return RSAObjectToStruct(cspObject);
        }

        [SecurityCritical]
        private void GetKeyPair()
        {
            if (this._safeKeyHandle == null)
            {
                lock (this)
                {
                    if (this._safeKeyHandle == null)
                    {
                        Utils.GetKeyPairHelper(CspAlgorithmType.Rsa, this._parameters, this._randomKeyContainer, this._dwKeySize, ref this._safeProvHandle, ref this._safeKeyHandle);
                    }
                }
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void ImportCspBlob(byte[] keyBlob)
        {
            Utils.ImportCspBlobHelper(CspAlgorithmType.Rsa, keyBlob, IsPublic(keyBlob), ref this._parameters, this._randomKeyContainer, ref this._safeProvHandle, ref this._safeKeyHandle);
        }

        [SecuritySafeCritical]
        public override void ImportParameters(RSAParameters parameters)
        {
            if ((this._safeKeyHandle != null) && !this._safeKeyHandle.IsClosed)
            {
                this._safeKeyHandle.Dispose();
                this._safeKeyHandle = null;
            }
            RSACspObject cspObject = RSAStructToObject(parameters);
            this._safeKeyHandle = SafeKeyHandle.InvalidHandle;
            if (IsPublic(parameters))
            {
                Utils._ImportKey(Utils.StaticProvHandle, 0xa400, CspProviderFlags.NoFlags, cspObject, ref this._safeKeyHandle);
            }
            else
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Import);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
                if (this._safeProvHandle == null)
                {
                    this._safeProvHandle = Utils.CreateProvHandle(this._parameters, this._randomKeyContainer);
                }
                Utils._ImportKey(this._safeProvHandle, 0xa400, this._parameters.Flags, cspObject, ref this._safeKeyHandle);
            }
        }

        private static bool IsPublic(byte[] keyBlob)
        {
            if (keyBlob == null)
            {
                throw new ArgumentNullException("keyBlob");
            }
            if (keyBlob[0] != 6)
            {
                return false;
            }
            return (((keyBlob[11] == 0x31) && (keyBlob[10] == 0x41)) && ((keyBlob[9] == 0x53) && (keyBlob[8] == 0x52)));
        }

        private static bool IsPublic(RSAParameters rsaParams)
        {
            return (rsaParams.P == null);
        }

        private static RSAParameters RSAObjectToStruct(RSACspObject rsaCspObject)
        {
            return new RSAParameters { Exponent = rsaCspObject.Exponent, Modulus = rsaCspObject.Modulus, P = rsaCspObject.P, Q = rsaCspObject.Q, DP = rsaCspObject.DP, DQ = rsaCspObject.DQ, InverseQ = rsaCspObject.InverseQ, D = rsaCspObject.D };
        }

        private static RSACspObject RSAStructToObject(RSAParameters rsaParams)
        {
            return new RSACspObject { Exponent = rsaParams.Exponent, Modulus = rsaParams.Modulus, P = rsaParams.P, Q = rsaParams.Q, DP = rsaParams.DP, DQ = rsaParams.DQ, InverseQ = rsaParams.InverseQ, D = rsaParams.D };
        }

        [SecuritySafeCritical]
        public byte[] SignData(Stream inputStream, object halg)
        {
            string str = Utils.ObjToOidValue(halg);
            byte[] rgbHash = Utils.ObjToHashAlgorithm(halg).ComputeHash(inputStream);
            return this.SignHash(rgbHash, str);
        }

        [SecuritySafeCritical]
        public byte[] SignData(byte[] buffer, object halg)
        {
            string str = Utils.ObjToOidValue(halg);
            byte[] rgbHash = Utils.ObjToHashAlgorithm(halg).ComputeHash(buffer);
            return this.SignHash(rgbHash, str);
        }

        [SecuritySafeCritical]
        public byte[] SignData(byte[] buffer, int offset, int count, object halg)
        {
            string str = Utils.ObjToOidValue(halg);
            byte[] rgbHash = Utils.ObjToHashAlgorithm(halg).ComputeHash(buffer, offset, count);
            return this.SignHash(rgbHash, str);
        }

        [SecuritySafeCritical]
        public byte[] SignHash(byte[] rgbHash, string str)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (this.PublicOnly)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NoPrivateKey"));
            }
            int calgHash = X509Utils.OidToAlgId(str);
            this.GetKeyPair();
            if (!this.CspKeyContainerInfo.RandomlyGenerated)
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Sign);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            return Utils.SignValue(this._safeKeyHandle, this._parameters.KeyNumber, 0x2400, calgHash, rgbHash);
        }

        [SecuritySafeCritical]
        public bool VerifyData(byte[] buffer, object halg, byte[] signature)
        {
            string str = Utils.ObjToOidValue(halg);
            byte[] rgbHash = Utils.ObjToHashAlgorithm(halg).ComputeHash(buffer);
            return this.VerifyHash(rgbHash, str, signature);
        }

        [SecuritySafeCritical]
        internal bool VerifyHash(byte[] rgbHash, int calgHash, byte[] rgbSignature)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (rgbSignature == null)
            {
                throw new ArgumentNullException("rgbSignature");
            }
            this.GetKeyPair();
            return Utils.VerifySign(this._safeKeyHandle, 0x2400, calgHash, rgbHash, rgbSignature);
        }

        [SecuritySafeCritical]
        public bool VerifyHash(byte[] rgbHash, string str, byte[] rgbSignature)
        {
            if (rgbHash == null)
            {
                throw new ArgumentNullException("rgbHash");
            }
            if (rgbSignature == null)
            {
                throw new ArgumentNullException("rgbSignature");
            }
            this.GetKeyPair();
            int calgHash = X509Utils.OidToAlgId(str, OidGroup.HashAlgorithm);
            return this.VerifyHash(rgbHash, calgHash, rgbSignature);
        }

        [ComVisible(false)]
        public System.Security.Cryptography.CspKeyContainerInfo CspKeyContainerInfo
        {
            [SecuritySafeCritical]
            get
            {
                this.GetKeyPair();
                return new System.Security.Cryptography.CspKeyContainerInfo(this._parameters, this._randomKeyContainer);
            }
        }

        public override string KeyExchangeAlgorithm
        {
            get
            {
                if (this._parameters.KeyNumber == 1)
                {
                    return "RSA-PKCS1-KeyEx";
                }
                return null;
            }
        }

        public override int KeySize
        {
            [SecuritySafeCritical]
            get
            {
                this.GetKeyPair();
                byte[] buffer = Utils._GetKeyParameter(this._safeKeyHandle, 1);
                this._dwKeySize = ((buffer[0] | (buffer[1] << 8)) | (buffer[2] << 0x10)) | (buffer[3] << 0x18);
                return this._dwKeySize;
            }
        }

        public bool PersistKeyInCsp
        {
            [SecuritySafeCritical]
            get
            {
                if (this._safeProvHandle == null)
                {
                    lock (this)
                    {
                        if (this._safeProvHandle == null)
                        {
                            this._safeProvHandle = Utils.CreateProvHandle(this._parameters, this._randomKeyContainer);
                        }
                    }
                }
                return Utils.GetPersistKeyInCsp(this._safeProvHandle);
            }
            [SecuritySafeCritical]
            set
            {
                bool persistKeyInCsp = this.PersistKeyInCsp;
                if (value != persistKeyInCsp)
                {
                    KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                    if (!value)
                    {
                        KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Delete);
                        permission.AccessEntries.Add(accessEntry);
                    }
                    else
                    {
                        KeyContainerPermissionAccessEntry entry2 = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Create);
                        permission.AccessEntries.Add(entry2);
                    }
                    permission.Demand();
                    Utils.SetPersistKeyInCsp(this._safeProvHandle, value);
                }
            }
        }

        [ComVisible(false)]
        public bool PublicOnly
        {
            [SecuritySafeCritical]
            get
            {
                this.GetKeyPair();
                return (Utils._GetKeyParameter(this._safeKeyHandle, 2)[0] == 1);
            }
        }

        public override string SignatureAlgorithm
        {
            get
            {
                return "http://www.w3.org/2000/09/xmldsig#rsa-sha1";
            }
        }

        public static bool UseMachineKeyStore
        {
            get
            {
                return (s_UseMachineKeyStore == CspProviderFlags.UseMachineKeyStore);
            }
            set
            {
                s_UseMachineKeyStore = value ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags;
            }
        }
    }
}

