namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeKeyHandle() : base(true)
        {
            base.SetHandle(IntPtr.Zero);
        }

        private SafeKeyHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void FreeKey(IntPtr pKeyCotext);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            FreeKey(base.handle);
            return true;
        }

        internal static SafeKeyHandle InvalidHandle
        {
            get
            {
                return new SafeKeyHandle();
            }
        }
    }
}

