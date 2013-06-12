namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeInternetHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeInternetHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        protected override bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.WinHttp.WinHttpCloseHandle(base.handle);
        }
    }
}

