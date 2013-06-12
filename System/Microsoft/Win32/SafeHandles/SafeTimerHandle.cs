namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeTimerHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeTimerHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true, ExactSpelling=true)]
        private static extern bool CloseHandle(IntPtr handle);
        protected override bool ReleaseHandle()
        {
            return CloseHandle(base.handle);
        }
    }
}

