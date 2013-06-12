namespace System.Security.Cryptography.X509Certificates
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeCertStoreHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCertStoreHandle() : base(true)
        {
        }

        internal SafeCertStoreHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern void _FreeCertStoreContext(IntPtr hCertStore);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            _FreeCertStoreContext(base.handle);
            return true;
        }

        internal static SafeCertStoreHandle InvalidHandle
        {
            get
            {
                return new SafeCertStoreHandle(IntPtr.Zero);
            }
        }
    }
}

