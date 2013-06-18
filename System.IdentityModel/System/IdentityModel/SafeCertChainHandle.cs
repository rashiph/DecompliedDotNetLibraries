namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class SafeCertChainHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCertChainHandle() : base(true)
        {
        }

        private SafeCertChainHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            CAPI.CertFreeCertificateChain(base.handle);
            return true;
        }

        internal static SafeCertChainHandle InvalidHandle
        {
            get
            {
                return new SafeCertChainHandle(IntPtr.Zero);
            }
        }
    }
}

