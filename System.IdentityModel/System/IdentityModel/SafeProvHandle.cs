namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;

    internal class SafeProvHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeProvHandle() : base(true)
        {
        }

        private SafeProvHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CryptReleaseContext(base.handle, 0);
        }

        internal static SafeProvHandle InvalidHandle
        {
            get
            {
                return new SafeProvHandle(IntPtr.Zero);
            }
        }
    }
}

