namespace System.DirectoryServices.ActiveDirectory
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class PolicySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal PolicySafeHandle(IntPtr value) : base(true)
        {
            base.SetHandle(value);
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNativeMethods.LsaClose(base.handle) == 0);
        }
    }
}

