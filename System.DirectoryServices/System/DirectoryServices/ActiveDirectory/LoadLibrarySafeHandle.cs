namespace System.DirectoryServices.ActiveDirectory
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class LoadLibrarySafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private LoadLibrarySafeHandle() : base(true)
        {
        }

        internal LoadLibrarySafeHandle(IntPtr value) : base(true)
        {
            base.SetHandle(value);
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNativeMethods.FreeLibrary(base.handle) != 0);
        }
    }
}

