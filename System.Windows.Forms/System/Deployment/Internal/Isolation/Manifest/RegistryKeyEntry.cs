namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal class RegistryKeyEntry : IDisposable
    {
        public uint Flags;
        public uint Protection;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string BuildFilter;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr SecurityDescriptor;
        public uint SecurityDescriptorSize;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr Values;
        public uint ValuesSize;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr Keys;
        public uint KeysSize;
        ~RegistryKeyEntry()
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
            if (this.Values != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.Values);
                this.Values = IntPtr.Zero;
            }
            if (this.Keys != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.Keys);
                this.Keys = IntPtr.Zero;
            }
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}

