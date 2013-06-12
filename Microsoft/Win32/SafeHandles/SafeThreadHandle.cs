namespace Microsoft.Win32.SafeHandles
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeThreadHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeThreadHandle() : base(true)
        {
        }

        internal void InitialSetHandle(IntPtr h)
        {
            base.SetHandle(h);
        }

        protected override bool ReleaseHandle()
        {
            return Microsoft.Win32.SafeNativeMethods.CloseHandle(base.handle);
        }
    }
}

