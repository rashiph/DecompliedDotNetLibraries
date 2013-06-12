namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal sealed class SafeCapiKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCapiKeyHandle() : base(true)
        {
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32")]
        private static extern bool CryptDestroyKey(IntPtr hKey);
        internal SafeCapiKeyHandle Duplicate()
        {
            SafeCapiKeyHandle phKey = null;
            if (!CapiNative.UnsafeNativeMethods.CryptDuplicateKey(this, IntPtr.Zero, 0, out phKey))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return phKey;
        }

        protected override bool ReleaseHandle()
        {
            return CryptDestroyKey(base.handle);
        }

        internal static SafeCapiKeyHandle InvalidHandle
        {
            get
            {
                SafeCapiKeyHandle handle = new SafeCapiKeyHandle();
                handle.SetHandle(IntPtr.Zero);
                return handle;
            }
        }
    }
}

