namespace System.Net.NetworkInformation
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal class SafeFreeMibTable : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeFreeMibTable() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            UnsafeNetInfoNativeMethods.FreeMibTable(base.handle);
            base.handle = IntPtr.Zero;
            return true;
        }
    }
}

