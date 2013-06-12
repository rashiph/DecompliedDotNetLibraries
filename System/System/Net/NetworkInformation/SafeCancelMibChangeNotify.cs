namespace System.Net.NetworkInformation
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal class SafeCancelMibChangeNotify : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeCancelMibChangeNotify() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            uint num = UnsafeNetInfoNativeMethods.CancelMibChangeNotify2(base.handle);
            base.handle = IntPtr.Zero;
            return (num == 0);
        }
    }
}

