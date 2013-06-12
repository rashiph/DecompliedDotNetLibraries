namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeLoadLibrary : SafeHandleZeroOrMinusOneIsInvalid
    {
        private const string KERNEL32 = "kernel32.dll";
        public static readonly SafeLoadLibrary Zero = new SafeLoadLibrary(false);

        private SafeLoadLibrary() : base(true)
        {
        }

        private SafeLoadLibrary(bool ownsHandle) : base(ownsHandle)
        {
        }

        public static SafeLoadLibrary LoadLibraryEx(string library)
        {
            SafeLoadLibrary library2 = ComNetOS.IsWin9x ? UnsafeNclNativeMethods.SafeNetHandles.LoadLibraryExA(library, null, 0) : UnsafeNclNativeMethods.SafeNetHandles.LoadLibraryExW(library, null, 0);
            if (library2.IsInvalid)
            {
                library2.SetHandleAsInvalid();
            }
            return library2;
        }

        protected override bool ReleaseHandle()
        {
            return UnsafeNclNativeMethods.SafeNetHandles.FreeLibrary(base.handle);
        }
    }
}

