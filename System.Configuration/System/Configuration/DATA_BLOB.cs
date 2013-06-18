namespace System.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DATA_BLOB : IDisposable
    {
        public int cbData;
        public IntPtr pbData;
        void IDisposable.Dispose()
        {
            if (this.pbData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.pbData);
                this.pbData = IntPtr.Zero;
            }
        }
    }
}

