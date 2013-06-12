namespace System.Security.Cryptography
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    [ComVisible(true)]
    public class PasswordDeriveBytes : DeriveBytes
    {
        private byte[] _baseValue;
        private CspParameters _cspParams;
        private byte[] _extra;
        private int _extraCount;
        private HashAlgorithm _hash;
        private string _hashName;
        private int _iterations;
        private byte[] _password;
        private int _prefix;
        [SecurityCritical]
        private SafeProvHandle _safeProvHandle;
        private byte[] _salt;

        [SecuritySafeCritical]
        public PasswordDeriveBytes(string strPassword, byte[] rgbSalt) : this(strPassword, rgbSalt, new CspParameters())
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(byte[] password, byte[] salt) : this(password, salt, new CspParameters())
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, CspParameters cspParams) : this(strPassword, rgbSalt, "SHA1", 100, cspParams)
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(byte[] password, byte[] salt, CspParameters cspParams) : this(password, salt, "SHA1", 100, cspParams)
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, string strHashName, int iterations) : this(strPassword, rgbSalt, strHashName, iterations, new CspParameters())
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(byte[] password, byte[] salt, string hashName, int iterations) : this(password, salt, hashName, iterations, new CspParameters())
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(string strPassword, byte[] rgbSalt, string strHashName, int iterations, CspParameters cspParams) : this(new UTF8Encoding(false).GetBytes(strPassword), rgbSalt, strHashName, iterations, cspParams)
        {
        }

        [SecuritySafeCritical]
        public PasswordDeriveBytes(byte[] password, byte[] salt, string hashName, int iterations, CspParameters cspParams)
        {
            this.IterationCount = iterations;
            this.Salt = salt;
            this.HashName = hashName;
            this._password = password;
            this._cspParams = cspParams;
        }

        private byte[] ComputeBaseValue()
        {
            this._hash.Initialize();
            this._hash.TransformBlock(this._password, 0, this._password.Length, this._password, 0);
            if (this._salt != null)
            {
                this._hash.TransformBlock(this._salt, 0, this._salt.Length, this._salt, 0);
            }
            this._hash.TransformFinalBlock(new byte[0], 0, 0);
            this._baseValue = this._hash.Hash;
            this._hash.Initialize();
            for (int i = 1; i < (this._iterations - 1); i++)
            {
                this._hash.ComputeHash(this._baseValue);
                this._baseValue = this._hash.Hash;
            }
            return this._baseValue;
        }

        [SecurityCritical]
        private byte[] ComputeBytes(int cb)
        {
            int dstOffsetBytes = 0;
            this._hash.Initialize();
            int byteCount = this._hash.HashSize / 8;
            byte[] dst = new byte[(((cb + byteCount) - 1) / byteCount) * byteCount];
            using (CryptoStream stream = new CryptoStream(Stream.Null, this._hash, CryptoStreamMode.Write))
            {
                this.HashPrefix(stream);
                stream.Write(this._baseValue, 0, this._baseValue.Length);
                stream.Close();
            }
            Buffer.InternalBlockCopy(this._hash.Hash, 0, dst, dstOffsetBytes, byteCount);
            for (dstOffsetBytes += byteCount; cb > dstOffsetBytes; dstOffsetBytes += byteCount)
            {
                this._hash.Initialize();
                using (CryptoStream stream2 = new CryptoStream(Stream.Null, this._hash, CryptoStreamMode.Write))
                {
                    this.HashPrefix(stream2);
                    stream2.Write(this._baseValue, 0, this._baseValue.Length);
                    stream2.Close();
                }
                Buffer.InternalBlockCopy(this._hash.Hash, 0, dst, dstOffsetBytes, byteCount);
            }
            return dst;
        }

        [SecuritySafeCritical]
        public byte[] CryptDeriveKey(string algname, string alghashname, int keySize, byte[] rgbIV)
        {
            if (keySize < 0)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeySize"));
            }
            int algidHash = X509Utils.OidToAlgId(alghashname);
            if (algidHash == 0)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidAlgorithm"));
            }
            int algid = X509Utils.OidToAlgId(algname);
            if (algid == 0)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidAlgorithm"));
            }
            if (rgbIV == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_InvalidIV"));
            }
            byte[] o = null;
            DeriveKey(this.ProvHandle, algid, algidHash, this._password, this._password.Length, keySize << 0x10, rgbIV, rgbIV.Length, JitHelpers.GetObjectHandleOnStack<byte[]>(ref o));
            return o;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void DeriveKey(SafeProvHandle hProv, int algid, int algidHash, byte[] password, int cbPassword, int dwFlags, byte[] IV, int cbIV, ObjectHandleOnStack retKey);
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this._hash != null)
                {
                    this._hash.Dispose();
                }
                if (this._baseValue != null)
                {
                    Array.Clear(this._baseValue, 0, this._baseValue.Length);
                }
                if (this._extra != null)
                {
                    Array.Clear(this._extra, 0, this._extra.Length);
                }
                if (this._password != null)
                {
                    Array.Clear(this._password, 0, this._password.Length);
                }
                if (this._salt != null)
                {
                    Array.Clear(this._salt, 0, this._salt.Length);
                }
            }
        }

        [Obsolete("Rfc2898DeriveBytes replaces PasswordDeriveBytes for deriving key material from a password and is preferred in new applications."), SecuritySafeCritical]
        public override byte[] GetBytes(int cb)
        {
            int srcOffsetBytes = 0;
            byte[] dst = new byte[cb];
            if (this._baseValue == null)
            {
                this.ComputeBaseValue();
            }
            else if (this._extra != null)
            {
                srcOffsetBytes = this._extra.Length - this._extraCount;
                if (srcOffsetBytes >= cb)
                {
                    Buffer.InternalBlockCopy(this._extra, this._extraCount, dst, 0, cb);
                    if (srcOffsetBytes > cb)
                    {
                        this._extraCount += cb;
                        return dst;
                    }
                    this._extra = null;
                    return dst;
                }
                Buffer.InternalBlockCopy(this._extra, srcOffsetBytes, dst, 0, srcOffsetBytes);
                this._extra = null;
            }
            byte[] src = this.ComputeBytes(cb - srcOffsetBytes);
            Buffer.InternalBlockCopy(src, 0, dst, srcOffsetBytes, cb - srcOffsetBytes);
            if ((src.Length + srcOffsetBytes) > cb)
            {
                this._extra = src;
                this._extraCount = cb - srcOffsetBytes;
            }
            return dst;
        }

        private void HashPrefix(CryptoStream cs)
        {
            int index = 0;
            byte[] buffer = new byte[] { 0x30, 0x30, 0x30 };
            if (this._prefix > 0x3e7)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_TooManyBytes"));
            }
            if (this._prefix >= 100)
            {
                buffer[0] = (byte) (buffer[0] + ((byte) (this._prefix / 100)));
                index++;
            }
            if (this._prefix >= 10)
            {
                buffer[index] = (byte) (buffer[index] + ((byte) ((this._prefix % 100) / 10)));
                index++;
            }
            if (this._prefix > 0)
            {
                buffer[index] = (byte) (buffer[index] + ((byte) (this._prefix % 10)));
                index++;
                cs.Write(buffer, 0, index);
            }
            this._prefix++;
        }

        public override void Reset()
        {
            this._prefix = 0;
            this._extra = null;
            this._baseValue = null;
        }

        public string HashName
        {
            get
            {
                return this._hashName;
            }
            [SecuritySafeCritical]
            set
            {
                if (this._baseValue != null)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed", new object[] { "HashName" }));
                }
                this._hashName = value;
                this._hash = (HashAlgorithm) CryptoConfig.CreateFromName(this._hashName);
            }
        }

        public int IterationCount
        {
            get
            {
                return this._iterations;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
                }
                if (this._baseValue != null)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed", new object[] { "IterationCount" }));
                }
                this._iterations = value;
            }
        }

        private SafeProvHandle ProvHandle
        {
            [SecurityCritical]
            get
            {
                if (this._safeProvHandle == null)
                {
                    lock (this)
                    {
                        if (this._safeProvHandle == null)
                        {
                            SafeProvHandle handle = Utils.AcquireProvHandle(this._cspParams);
                            Thread.MemoryBarrier();
                            this._safeProvHandle = handle;
                        }
                    }
                }
                return this._safeProvHandle;
            }
        }

        public byte[] Salt
        {
            get
            {
                if (this._salt == null)
                {
                    return null;
                }
                return (byte[]) this._salt.Clone();
            }
            set
            {
                if (this._baseValue != null)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_ValuesFixed", new object[] { "Salt" }));
                }
                if (value == null)
                {
                    this._salt = null;
                }
                else
                {
                    this._salt = (byte[]) value.Clone();
                }
            }
        }
    }
}

