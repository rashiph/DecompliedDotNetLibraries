namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class SafeBerval
    {
        public int bv_len;
        public IntPtr bv_val = IntPtr.Zero;
        ~SafeBerval()
        {
            if (this.bv_val != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(this.bv_val);
            }
        }
    }
}

