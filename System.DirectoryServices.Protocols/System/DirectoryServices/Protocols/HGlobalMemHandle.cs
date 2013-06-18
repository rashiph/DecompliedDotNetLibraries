namespace System.DirectoryServices.Protocols
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class HGlobalMemHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal HGlobalMemHandle(IntPtr value) : base(true)
        {
            base.SetHandle(value);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(base.handle);
            return true;
        }
    }
}

