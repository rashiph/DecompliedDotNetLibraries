namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal class DirectoryEntry : IDisposable
    {
        public uint Flags;
        public uint Protection;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string BuildFilter;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr SecurityDescriptor;
        public uint SecurityDescriptorSize;
        ~DirectoryEntry()
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
            if (this.SecurityDescriptor != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.SecurityDescriptor);
                this.SecurityDescriptor = IntPtr.Zero;
            }
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}

