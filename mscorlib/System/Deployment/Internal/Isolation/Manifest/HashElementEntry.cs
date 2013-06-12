namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal class HashElementEntry : IDisposable
    {
        public uint index;
        public byte Transform;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr TransformMetadata;
        public uint TransformMetadataSize;
        public byte DigestMethod;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr DigestValue;
        public uint DigestValueSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Xml;
        ~HashElementEntry()
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
            if (this.TransformMetadata != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.TransformMetadata);
                this.TransformMetadata = IntPtr.Zero;
            }
            if (this.DigestValue != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.DigestValue);
                this.DigestValue = IntPtr.Zero;
            }
            if (fDisposing)
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}

