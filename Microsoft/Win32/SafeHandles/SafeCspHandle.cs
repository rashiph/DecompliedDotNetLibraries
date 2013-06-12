namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeCspHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCspHandle() : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32", SetLastError=true)]
        private static extern bool CryptContextAddRef(SafeCspHandle hProv, IntPtr pdwReserved, int dwFlags);
        [return: MarshalAs(UnmanagedType.Bool)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("advapi32")]
        private static extern bool CryptReleaseContext(IntPtr hProv, int dwFlags);
        public SafeCspHandle Duplicate()
        {
            SafeCspHandle handle2;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = base.DangerousGetHandle();
                int hr = 0;
                SafeCspHandle handle = new SafeCspHandle();
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    if (!CryptContextAddRef(this, IntPtr.Zero, 0))
                    {
                        hr = Marshal.GetLastWin32Error();
                    }
                    else
                    {
                        handle.SetHandle(ptr);
                    }
                }
                if (hr != 0)
                {
                    handle.Dispose();
                    throw new CryptographicException(hr);
                }
                handle2 = handle;
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return handle2;
        }

        protected override bool ReleaseHandle()
        {
            return CryptReleaseContext(base.handle, 0);
        }
    }
}

