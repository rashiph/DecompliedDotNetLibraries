namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeLocalFree : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const int LMEM_FIXED = 0;
        private const int NULL = 0;
        public static SafeLocalFree Zero = new SafeLocalFree(false);

        private SafeLocalFree() : base(true)
        {
        }

        private SafeLocalFree(bool ownsHandle) : base(ownsHandle)
        {
        }

        public static SafeLocalFree LocalAlloc(int cb)
        {
            SafeLocalFree free = UnsafeNclNativeMethods.SafeNetHandles.LocalAlloc(0, (UIntPtr) cb);
            if (free.IsInvalid)
            {
                free.SetHandleAsInvalid();
                throw new OutOfMemoryException();
            }
            return free;
        }

        protected override bool ReleaseHandle()
        {
            return (UnsafeNclNativeMethods.SafeNetHandles.LocalFree(base.handle) == IntPtr.Zero);
        }
    }
}

