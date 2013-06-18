namespace System.ServiceModel.Channels
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeFileMappingHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNativeMethods.CloseHandle(base.handle) != 0);
        }
    }
}

