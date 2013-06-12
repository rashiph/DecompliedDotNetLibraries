namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeHashHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeHashHandle() : base(true)
        {
            base.SetHandle(IntPtr.Zero);
        }

        private SafeHashHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void FreeHash(IntPtr pHashContext);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            FreeHash(base.handle);
            return true;
        }

        internal static SafeHashHandle InvalidHandle
        {
            get
            {
                return new SafeHashHandle();
            }
        }
    }
}

