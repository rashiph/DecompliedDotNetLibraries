namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class SafeCryptProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCryptProvHandle() : base(true)
        {
        }

        internal SafeCryptProvHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        private static extern bool CryptReleaseContext(IntPtr hCryptProv, uint dwFlags);
        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(base.handle, 0);
        }

        internal static SafeCryptProvHandle InvalidHandle
        {
            get
            {
                return new SafeCryptProvHandle(IntPtr.Zero);
            }
        }
    }
}

