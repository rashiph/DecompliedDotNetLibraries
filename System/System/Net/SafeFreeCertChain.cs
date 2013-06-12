namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFreeCertChain : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string CRYPT32 = "crypt32.dll";

        internal SafeFreeCertChain(IntPtr handle) : base(false)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            UnsafeNclNativeMethods.SafeNetHandles.CertFreeCertificateChain(base.handle);
            return true;
        }

        public override string ToString()
        {
            return ("0x" + base.DangerousGetHandle().ToString("x"));
        }
    }
}

