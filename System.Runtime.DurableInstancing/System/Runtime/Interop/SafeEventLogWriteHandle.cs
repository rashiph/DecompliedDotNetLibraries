namespace System.Runtime.Interop
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
        public static System.Runtime.Interop.SafeEventLogWriteHandle RegisterEventSource(string uncServerName, string sourceName)
        {
            System.Runtime.Interop.SafeEventLogWriteHandle handle = UnsafeNativeMethods.RegisterEventSource(uncServerName, sourceName);
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

