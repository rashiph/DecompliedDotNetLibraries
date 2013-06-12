namespace Microsoft.Win32.SafeHandles
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [SuppressUnmanagedCodeSecurity, HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal sealed class SafeEventLogReadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeEventLogReadHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("advapi32.dll", SetLastError=true)]
        private static extern bool CloseEventLog(IntPtr hEventLog);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern SafeEventLogReadHandle OpenEventLog(string UNCServerName, string sourceName);
        protected override bool ReleaseHandle()
        {
            return CloseEventLog(base.handle);
        }
    }
}

