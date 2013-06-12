namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ComVisible(true)]
    public class Rfc2898DeriveBytes : DeriveBytes
    {
        private const int BlockSize = 20;
        private uint m_block;
        private byte[] m_buffer;
        private int m_endIndex;
        private HMACSHA1 m_hmacsha1;
        private uint m_iterations;
        private byte[] m_salt;
        private int m_startIndex;

        public Rfc2898DeriveBytes(string password, int saltSize) : this(password, saltSize, 0x3e8)
        {
        }

        public Rfc2898DeriveBytes(string password, byte[] salt) : this(password, salt, 0x3e8)
        {
        }

        public Rfc2898DeriveBytes(string password, int saltSize, int iterations)
        {
            if (saltSize < 0)
            {
                throw new ArgumentOutOfRangeException("saltSize", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            byte[] data = new byte[saltSize];
            Utils.StaticRandomNumberGenerator.GetBytes(data);
            this.Salt = data;
            this.IterationCount = iterations;
            this.m_hmacsha1 = new HMACSHA1(new UTF8Encoding(false).GetBytes(password));
            this.Initialize();
        }

        public Rfc2898DeriveBytes(string password, byte[] salt, int iterations) : this(new UTF8Encoding(false).GetBytes(password), salt, iterations)
        {
        }

        public Rfc2898DeriveBytes(byte[] password, byte[] salt, int iterations)
        {
            this.Salt = salt;
            this.IterationCount = iterations;
            this.m_hmacsha1 = new HMACSHA1(password);
            this.Initialize();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (this.m_hmacsha1 != null)
                {
                    this.m_hmacsha1.Dispose();
                }
                if (this.m_buffer != null)
                {
                    Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
                }
                if (this.m_salt != null)
                {
                    Array.Clear(this.m_salt, 0, this.m_salt.Length);
                }
            }
        }

        private byte[] Func()
        {
            byte[] inputBuffer = Utils.Int(this.m_block);
            this.m_hmacsha1.TransformBlock(this.m_salt, 0, this.m_salt.Length, this.m_salt, 0);
            this.m_hmacsha1.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            byte[] hash = this.m_hmacsha1.Hash;
            this.m_hmacsha1.Initialize();
            byte[] buffer3 = hash;
            for (int i = 2; i <= this.m_iterations; i++)
            {
                hash = this.m_hmacsha1.ComputeHash(hash);
                for (int j = 0; j < 20; j++)
                {
                    buffer3[j] = (byte) (buffer3[j] ^ hash[j]);
                }
            }
            this.m_block++;
            return buffer3;
        }

        [SecuritySafeCritical]
        public override byte[] GetBytes(int cb)
        {
            if (cb <= 0)
            {
                throw new ArgumentOutOfRangeException("cb", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            byte[] dst = new byte[cb];
            int dstOffsetBytes = 0;
            int byteCount = this.m_endIndex - this.m_startIndex;
            if (byteCount > 0)
            {
                if (cb < byteCount)
                {
                    Buffer.InternalBlockCopy(this.m_buffer, this.m_startIndex, dst, 0, cb);
                    this.m_startIndex += cb;
                    return dst;
                }
                Buffer.InternalBlockCopy(this.m_buffer, this.m_startIndex, dst, 0, byteCount);
                this.m_startIndex = this.m_endIndex = 0;
                dstOffsetBytes += byteCount;
            }
            while (dstOffsetBytes < cb)
            {
                byte[] src = this.Func();
                int num3 = cb - dstOffsetBytes;
                if (num3 > 20)
                {
                    Buffer.InternalBlockCopy(src, 0, dst, dstOffsetBytes, 20);
                    dstOffsetBytes += 20;
                }
                else
                {
                    Buffer.InternalBlockCopy(src, 0, dst, dstOffsetBytes, num3);
                    dstOffsetBytes += num3;
                    Buffer.InternalBlockCopy(src, num3, this.m_buffer, this.m_startIndex, 20 - num3);
                    this.m_endIndex += 20 - num3;
                    return dst;
                }
            }
            return dst;
        }

        private void Initialize()
        {
            if (this.m_buffer != null)
            {
                Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
            }
            this.m_buffer = new byte[20];
            this.m_block = 1;
            this.m_startIndex = this.m_endIndex = 0;
        }

        public override void Reset()
        {
            this.Initialize();
        }

        public int IterationCount
        {
            get
            {
                return (int) this.m_iterations;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
                }
                this.m_iterations = (uint) value;
                this.Initialize();
            }
        }

        public byte[] Salt
        {
            get
            {
                return (byte[]) this.m_salt.Clone();
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length < 8)
                {
                    throw new ArgumentException(Environment.GetResourceString("Cryptography_PasswordDerivedBytes_FewBytesSalt"));
                }
                this.m_salt = (byte[]) value.Clone();
                this.Initialize();
            }
        }
    }
}

