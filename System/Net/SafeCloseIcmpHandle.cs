namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Net.NetworkInformation;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeCloseIcmpHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeCloseIcmpHandle() : base(true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return UnsafeNetInfoNativeMethods.IcmpCloseHandle(base.handle);
        }
    }
}

