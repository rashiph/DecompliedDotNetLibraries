namespace System.IdentityModel
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeHGlobalHandle() : base(true)
        {
        }

        private SafeHGlobalHandle(IntPtr handle) : base(true)
        {
            base.SetHandle(handle);
        }

        public static SafeHGlobalHandle AllocHGlobal(byte[] bytes)
        {
            SafeHGlobalHandle handle = AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, handle.DangerousGetHandle(), bytes.Length);
            return handle;
        }

        public static SafeHGlobalHandle AllocHGlobal(int cb)
        {
            if (cb < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("cb", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
            }
            SafeHGlobalHandle handle = new SafeHGlobalHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr ptr = Marshal.AllocHGlobal(cb);
                handle.SetHandle(ptr);
            }
            return handle;
        }

        public static SafeHGlobalHandle AllocHGlobal(string s)
        {
            byte[] bytes = DiagnosticUtility.Utility.AllocateByteArray((s.Length + 1) * 2);
            Encoding.Unicode.GetBytes(s, 0, s.Length, bytes, 0);
            return AllocHGlobal(bytes);
        }

        public static SafeHGlobalHandle AllocHGlobal(uint cb)
        {
            return AllocHGlobal((int) cb);
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(base.handle);
            return true;
        }

        public static SafeHGlobalHandle InvalidHandle
        {
            get
            {
                return new SafeHGlobalHandle(IntPtr.Zero);
            }
        }
    }
}

