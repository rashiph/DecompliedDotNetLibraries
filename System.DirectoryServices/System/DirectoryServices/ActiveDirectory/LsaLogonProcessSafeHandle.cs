namespace System.DirectoryServices.ActiveDirectory
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class LsaLogonProcessSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private LsaLogonProcessSafeHandle() : base(true)
        {
        }

        internal LsaLogonProcessSafeHandle(IntPtr value) : base(true)
        {
            base.SetHandle(value);
        }

        protected override bool ReleaseHandle()
        {
            return (NativeMethods.LsaDeregisterLogonProcess(base.handle) == 0);
        }
    }
}

