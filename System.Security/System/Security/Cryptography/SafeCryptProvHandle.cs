namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeCryptProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCryptProvHandle() : base(true)
        {
        }

        internal SafeCryptProvHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32.dll", SetLastError=true)]
        private static extern bool CryptReleaseContext(IntPtr hCryptProv, uint dwFlags);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(base.handle, 0);
        }

        internal static System.Security.Cryptography.SafeCryptProvHandle InvalidHandle
        {
            get
            {
                return new System.Security.Cryptography.SafeCryptProvHandle(IntPtr.Zero);
            }
        }
    }
}

