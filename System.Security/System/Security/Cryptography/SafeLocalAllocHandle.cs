namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeLocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLocalAllocHandle() : base(true)
        {
        }

        internal SafeLocalAllocHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        private static extern IntPtr LocalFree(IntPtr handle);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (LocalFree(base.handle) == IntPtr.Zero);
        }

        internal static System.Security.Cryptography.SafeLocalAllocHandle InvalidHandle
        {
            get
            {
                return new System.Security.Cryptography.SafeLocalAllocHandle(IntPtr.Zero);
            }
        }
    }
}

