namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeCapiHashHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCapiHashHandle() : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32")]
        private static extern bool CryptDestroyHash(IntPtr hHash);
        protected override bool ReleaseHandle()
        {
            return CryptDestroyHash(base.handle);
        }

        public static SafeCapiHashHandle InvalidHandle
        {
            get
            {
                SafeCapiHashHandle handle = new SafeCapiHashHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }
    }
}

