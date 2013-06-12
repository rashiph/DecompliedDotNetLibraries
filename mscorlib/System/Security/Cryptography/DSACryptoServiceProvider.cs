namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [ComVisible(true)]
    public sealed class DSACryptoServiceProvider : DSA, ICspAsymmetricAlgorithm
    {
        private int _dwKeySize;
        private CspParameters _parameters;
        private bool _randomKeyContainer;
        [SecurityCritical]
        private SafeKeyHandle _safeKeyHandle;
        [SecurityCritical]
        private SafeProvHandle _safeProvHandle;
        private SHA1CryptoServiceProvider _sha1;
        private static CspProviderFlags s_UseMachineKeyStore;

        public DSACryptoServiceProvider() : this(0, new CspParameters(13, null, null, s_UseMachineKeyStore))
        {
        }

        public DSACryptoServiceProvider(int dwKeySize) : this(dwKeySize, new CspParameters(13, null, null, s_UseMachineKeyStore))
        {
        }

        public DSACryptoServiceProvider(CspParameters parameters) : this(0, parameters)
        {
        }

        [SecuritySafeCritical]
        public DSACryptoServiceProvider(int dwKeySize, CspParameters parameters)
        {
            if (dwKeySize < 0)
            {
                throw new ArgumentOutOfRangeException("dwKeySize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this._parameters = Utils.SaveCspParameters(CspAlgorithmType.Dss, parameters, s_UseMachineKeyStore, ref this._randomKeyContainer);
            base.LegalKeySizesValue = new KeySizes[] { new KeySizes(0x200, 0x400, 0x40) };
            this._dwKeySize = dwKeySize;
            this._sha1 = new SHA1CryptoServiceProvider();
            if (!this._randomKeyContainer || Environment.GetCompatibilityFlag(CompatibilityFlag.EagerlyGenerateRandomAsymmKeys))
            {
                this.GetKeyPair();
            }
        }

        [SecuritySafeCritical]
        public override byte[] CreateSignature(byte[] rgbHash)
        {
            return this.SignHash(rgbHash, null);
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

        private static DSAParameters DSAObjectToStruct(DSACspObject dsaCspObject)
        {
            return new DSAParameters { P = dsaCspObject.P, Q = dsaCspObject.Q, G = dsaCspObject.G, Y = dsaCspObject.Y, J = dsaCspObject.J, X = dsaCspObject.X, Seed = dsaCspObject.Seed, Counter = dsaCspObject.Counter };
        }

        private static DSACspObject DSAStructToObject(DSAParameters dsaParams)
        {
            return new DSACspObject { P = dsaParams.P, Q = dsaParams.Q, G = dsaParams.G, Y = dsaParams.Y, J = dsaParams.J, X = dsaParams.X, Seed = dsaParams.Seed, Counter = dsaParams.Counter };
        }

        [SecuritySafeCritical, ComVisible(false)]
        public byte[] ExportCspBlob(bool includePrivateParameters)
        {
            this.GetKeyPair();
            return Utils.ExportCspBlobHelper(includePrivateParameters, this._parameters, this._safeKeyHandle);
        }

        [SecuritySafeCritical]
        public override DSAParameters ExportParameters(bool includePrivateParameters)
        {
            this.GetKeyPair();
            if (includePrivateParameters)
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Export);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            DSACspObject cspObject = new DSACspObject();
            int blobType = includePrivateParameters ? 7 : 6;
            Utils._ExportKey(this._safeKeyHandle, blobType, cspObject);
            return DSAObjectToStruct(cspObject);
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
                        Utils.GetKeyPairHelper(CspAlgorithmType.Dss, this._parameters, this._randomKeyContainer, this._dwKeySize, ref this._safeProvHandle, ref this._safeKeyHandle);
                    }
                }
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void ImportCspBlob(byte[] keyBlob)
        {
            Utils.ImportCspBlobHelper(CspAlgorithmType.Dss, keyBlob, IsPublic(keyBlob), ref this._parameters, this._randomKeyContainer, ref this._safeProvHandle, ref this._safeKeyHandle);
        }

        [SecuritySafeCritical]
        public override void ImportParameters(DSAParameters parameters)
        {
            DSACspObject cspObject = DSAStructToObject(parameters);
            if ((this._safeKeyHandle != null) && !this._safeKeyHandle.IsClosed)
            {
                this._safeKeyHandle.Dispose();
            }
            this._safeKeyHandle = SafeKeyHandle.InvalidHandle;
            if (IsPublic(parameters))
            {
                Utils._ImportKey(Utils.StaticDssProvHandle, 0x2200, CspProviderFlags.NoFlags, cspObject, ref this._safeKeyHandle);
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
                Utils._ImportKey(this._safeProvHandle, 0x2200, this._parameters.Flags, cspObject, ref this._safeKeyHandle);
            }
        }

        private static bool IsPublic(DSAParameters dsaParams)
        {
            return (dsaParams.X == null);
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
            return (((keyBlob[11] == 0x31) || (keyBlob[11] == 0x33)) && (((keyBlob[10] == 0x53) && (keyBlob[9] == 0x53)) && (keyBlob[8] == 0x44)));
        }

        [SecuritySafeCritical]
        public byte[] SignData(Stream inputStream)
        {
            byte[] rgbHash = this._sha1.ComputeHash(inputStream);
            return this.SignHash(rgbHash, null);
        }

        [SecuritySafeCritical]
        public byte[] SignData(byte[] buffer)
        {
            byte[] rgbHash = this._sha1.ComputeHash(buffer);
            return this.SignHash(rgbHash, null);
        }

        [SecuritySafeCritical]
        public byte[] SignData(byte[] buffer, int offset, int count)
        {
            byte[] rgbHash = this._sha1.ComputeHash(buffer, offset, count);
            return this.SignHash(rgbHash, null);
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
            if (rgbHash.Length != (this._sha1.HashSize / 8))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHashSize", new object[] { "SHA1", this._sha1.HashSize / 8 }));
            }
            this.GetKeyPair();
            if (!this.CspKeyContainerInfo.RandomlyGenerated)
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this._parameters, KeyContainerPermissionFlags.Sign);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            return Utils.SignValue(this._safeKeyHandle, this._parameters.KeyNumber, 0x2200, calgHash, rgbHash);
        }

        [SecuritySafeCritical]
        public bool VerifyData(byte[] rgbData, byte[] rgbSignature)
        {
            byte[] rgbHash = this._sha1.ComputeHash(rgbData);
            return this.VerifyHash(rgbHash, null, rgbSignature);
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
            int calgHash = X509Utils.OidToAlgId(str);
            if (rgbHash.Length != (this._sha1.HashSize / 8))
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHashSize", new object[] { "SHA1", this._sha1.HashSize / 8 }));
            }
            this.GetKeyPair();
            return Utils.VerifySign(this._safeKeyHandle, 0x2200, calgHash, rgbHash, rgbSignature);
        }

        [SecuritySafeCritical]
        public override bool VerifySignature(byte[] rgbHash, byte[] rgbSignature)
        {
            return this.VerifyHash(rgbHash, null, rgbSignature);
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
                return "http://www.w3.org/2000/09/xmldsig#dsa-sha1";
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

