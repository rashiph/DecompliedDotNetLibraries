namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class CapiHashAlgorithm : IDisposable
    {
        private CapiNative.AlgorithmId m_algorithmId;
        private SafeCspHandle m_cspHandle;
        private SafeCapiHashHandle m_hashHandle;

        [SecurityCritical]
        public CapiHashAlgorithm(string provider, CapiNative.ProviderType providerType, CapiNative.AlgorithmId algorithm)
        {
            this.m_algorithmId = algorithm;
            this.m_cspHandle = CapiNative.AcquireCsp(null, provider, providerType, CapiNative.CryptAcquireContextFlags.None | CapiNative.CryptAcquireContextFlags.VerifyContext, true);
            this.Initialize();
        }

        [SecurityCritical]
        public void Dispose()
        {
            if (this.m_hashHandle != null)
            {
                this.m_hashHandle.Dispose();
            }
            if (this.m_cspHandle != null)
            {
                this.m_cspHandle.Dispose();
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
            if (!CapiNative.UnsafeNativeMethods.CryptHashData(this.m_hashHandle, dst, cbSize, 0))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        [SecurityCritical]
        public byte[] HashFinal()
        {
            return CapiNative.GetHashParameter(this.m_hashHandle, CapiNative.HashParameter.HashValue);
        }

        [SecurityCritical]
        public void Initialize()
        {
            SafeCapiHashHandle phHash = null;
            if (!CapiNative.UnsafeNativeMethods.CryptCreateHash(this.m_cspHandle, this.m_algorithmId, SafeCapiKeyHandle.InvalidHandle, 0, out phHash))
            {
                int hr = Marshal.GetLastWin32Error();
                if (hr == -2146893816)
                {
                    throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
                }
                throw new CryptographicException(hr);
            }
            if (this.m_hashHandle != null)
            {
                this.m_hashHandle.Dispose();
            }
            this.m_hashHandle = phHash;
        }
    }
}

