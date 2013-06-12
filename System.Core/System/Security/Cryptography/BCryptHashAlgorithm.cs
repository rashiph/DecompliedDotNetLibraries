namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class BCryptHashAlgorithm : IDisposable
    {
        private SafeBCryptAlgorithmHandle m_algorithmHandle;
        private SafeBCryptHashHandle m_hashHandle;

        [SecurityCritical]
        public BCryptHashAlgorithm(CngAlgorithm algorithm, string implementation)
        {
            if (!BCryptNative.BCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            this.m_algorithmHandle = BCryptNative.OpenAlgorithm(algorithm.Algorithm, implementation);
            this.Initialize();
        }

        [SecurityCritical]
        public void Dispose()
        {
            if (this.m_hashHandle != null)
            {
                this.m_hashHandle.Dispose();
            }
            if (this.m_algorithmHandle != null)
            {
                this.m_algorithmHandle.Dispose();
            }
        }

        [SecurityCritical]
        public void HashCore(byte[] array, int ibStart, int cbSize)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((ibStart < 0) || (ibStart > (array.Length - cbSize)))
            {
                throw new ArgumentOutOfRangeException("ibStart");
            }
            if ((cbSize < 0) || (cbSize > array.Length))
            {
                throw new ArgumentOutOfRangeException("cbSize");
            }
            byte[] dst = new byte[cbSize];
            Buffer.BlockCopy(array, ibStart, dst, 0, cbSize);
            BCryptNative.ErrorCode code = BCryptNative.UnsafeNativeMethods.BCryptHashData(this.m_hashHandle, dst, dst.Length, 0);
            if (code != BCryptNative.ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
        }

        [SecurityCritical]
        public byte[] HashFinal()
        {
            byte[] pbInput = new byte[BCryptNative.GetInt32Property<SafeBCryptHashHandle>(this.m_hashHandle, "HashDigestLength")];
            BCryptNative.ErrorCode code = BCryptNative.UnsafeNativeMethods.BCryptFinishHash(this.m_hashHandle, pbInput, pbInput.Length, 0);
            if (code != BCryptNative.ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return pbInput;
        }

        [SecurityCritical]
        public void HashStream(Stream stream)
        {
            byte[] buffer = new byte[0x1000];
            int cbSize = 0;
            do
            {
                cbSize = stream.Read(buffer, 0, buffer.Length);
                if (cbSize > 0)
                {
                    this.HashCore(buffer, 0, cbSize);
                }
            }
            while (cbSize > 0);
        }

        [SecurityCritical]
        public void Initialize()
        {
            SafeBCryptHashHandle phHash = null;
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                int cb = BCryptNative.GetInt32Property<SafeBCryptAlgorithmHandle>(this.m_algorithmHandle, "ObjectLength");
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    zero = Marshal.AllocCoTaskMem(cb);
                }
                BCryptNative.ErrorCode code = BCryptNative.UnsafeNativeMethods.BCryptCreateHash(this.m_algorithmHandle, out phHash, zero, cb, IntPtr.Zero, 0, 0);
                if (code != BCryptNative.ErrorCode.Success)
                {
                    throw new CryptographicException((int) code);
                }
                phHash.HashObject = zero;
            }
            finally
            {
                if ((zero != IntPtr.Zero) && ((phHash == null) || (phHash.HashObject == IntPtr.Zero)))
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            if (this.m_hashHandle != null)
            {
                this.m_hashHandle.Dispose();
            }
            this.m_hashHandle = phHash;
        }
    }
}

