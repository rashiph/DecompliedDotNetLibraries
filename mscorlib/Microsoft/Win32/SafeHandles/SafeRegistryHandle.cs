namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    public sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        internal SafeRegistryHandle() : base(true)
        {
        }

        [SecurityCritical]
        public SafeRegistryHandle(IntPtr preexistingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(preexistingHandle);
        }

        [SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll")]
        internal static extern int RegCloseKey(IntPtr hKey);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (RegCloseKey(base.handle) == 0);
        }
    }
}

