namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class RNGCryptoServiceProvider : RandomNumberGenerator
    {
        private bool m_ownsHandle;
        [SecurityCritical]
        private SafeProvHandle m_safeProvHandle;

        [SecuritySafeCritical]
        public RNGCryptoServiceProvider() : this((CspParameters) null)
        {
        }

        [SecuritySafeCritical]
        public RNGCryptoServiceProvider(byte[] rgb) : this((CspParameters) null)
        {
        }

        [SecuritySafeCritical]
        public RNGCryptoServiceProvider(CspParameters cspParams)
        {
            if (cspParams != null)
            {
                this.m_safeProvHandle = Utils.AcquireProvHandle(cspParams);
                this.m_ownsHandle = true;
            }
            else
            {
                this.m_safeProvHandle = Utils.StaticProvHandle;
                this.m_ownsHandle = false;
            }
        }

        [SecuritySafeCritical]
        public RNGCryptoServiceProvider(string str) : this((CspParameters) null)
        {
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && this.m_ownsHandle)
            {
                this.m_safeProvHandle.Dispose();
            }
        }

        [SecuritySafeCritical]
        public override void GetBytes(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            GetBytes(this.m_safeProvHandle, data, data.Length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetBytes(SafeProvHandle hProv, byte[] randomBytes, int count);
        [SecuritySafeCritical]
        public override void GetNonZeroBytes(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            GetNonZeroBytes(this.m_safeProvHandle, data, data.Length);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void GetNonZeroBytes(SafeProvHandle hProv, byte[] randomBytes, int count);
    }
}

