namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal class MuiResourceTypeIdIntEntry : IDisposable
    {
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr StringIds;
        public uint StringIdsSize;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr IntegerIds;
        public uint IntegerIdsSize;
        ~MuiResourceTypeIdIntEntry()
        {
            this.Dispose(false);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        [SecuritySafeCritical]
        public void Dispose(bool fDisposing)
        {
            if (this.StringIds != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.StringIds);
                this.StringIds = IntPtr.Zero;
            }
            if (this.IntegerIds != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.IntegerIds);
                this.IntegerIds = IntPtr.Zero;
            }
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}

