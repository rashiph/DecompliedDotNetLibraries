namespace System.Data.SqlTypes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class UnicodeString : SafeHandleZeroOrMinusOneIsInvalid
    {
        public UnicodeString(string path) : base(true)
        {
            this.Initialize(path);
        }

        private void Initialize(string path)
        {
            UnsafeNativeMethods.UNICODE_STRING unicode_string;
            unicode_string.length = (ushort) (path.Length * 2);
            unicode_string.maximumLength = (ushort) (path.Length * 2);
            unicode_string.buffer = path;
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                zero = Marshal.AllocHGlobal(Marshal.SizeOf(unicode_string));
                if (zero != IntPtr.Zero)
                {
                    base.SetHandle(zero);
                }
            }
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr handle = base.DangerousGetHandle();
                Marshal.StructureToPtr(unicode_string, handle, false);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        protected override bool ReleaseHandle()
        {
            if (base.handle != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(base.handle);
                base.handle = IntPtr.Zero;
            }
            return true;
        }
    }
}

