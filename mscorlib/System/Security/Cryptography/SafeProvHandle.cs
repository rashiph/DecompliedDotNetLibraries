namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeProvHandle() : base(true)
        {
            base.SetHandle(IntPtr.Zero);
        }

        private SafeProvHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void FreeCsp(IntPtr pProviderContext);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            FreeCsp(base.handle);
            return true;
        }

        internal static SafeProvHandle InvalidHandle
        {
            get
            {
                return new SafeProvHandle();
            }
        }
    }
}

