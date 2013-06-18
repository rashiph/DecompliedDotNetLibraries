namespace System.ServiceModel.Diagnostics
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class SafeEventLogWriteHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        private SafeEventLogWriteHandle() : base(true)
        {
        }

        [DllImport("advapi32", SetLastError=true)]
        private static extern bool DeregisterEventSource(IntPtr hEventLog);
        [SecurityCritical]
        internal static System.ServiceModel.Diagnostics.SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName)
        {
            System.ServiceModel.Diagnostics.SafeEventLogWriteHandle handle = System.ServiceModel.Diagnostics.NativeMethods.RegisterEventSource(uncServerName, sourceName);
            Marshal.GetLastWin32Error();
            bool isInvalid = handle.IsInvalid;
            return handle;
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return DeregisterEventSource(base.handle);
        }
    }
}

