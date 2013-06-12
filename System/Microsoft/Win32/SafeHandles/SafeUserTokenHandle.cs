namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeUserTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeUserTokenHandle() : base(true)
        {
        }

        internal SafeUserTokenHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
        {
            base.SetHandle(existingHandle);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool DuplicateTokenEx(SafeHandle hToken, int access, Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES tokenAttributes, int impersonationLevel, int tokenType, out SafeUserTokenHandle hNewToken);
        protected override bool ReleaseHandle()
        {
            return CloseHandle(base.handle);
        }
    }
}

