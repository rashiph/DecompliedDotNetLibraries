namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SecSizes
    {
        public int MaxToken;
        public int MaxSignature;
        public int BlockSize;
        public int SecurityTrailer;
        public static readonly int SizeOf = Marshal.SizeOf(typeof(SecSizes));
        internal unsafe SecSizes(byte[] memory)
        {
            byte[] buffer;
            if (((buffer = memory) != null) && (buffer.Length != 0))
            {
                goto Label_0015;
            }
            fixed (IntPtr* ptrRef = null)
            {
                IntPtr ptr;
                goto Label_001D;
            Label_0015:
                ptrRef = buffer;
            Label_001D:
                ptr = new IntPtr((void*) ptrRef);
                this.MaxToken = Marshal.ReadInt32(ptr);
                this.MaxSignature = Marshal.ReadInt32(ptr, 4);
                this.BlockSize = Marshal.ReadInt32(ptr, 8);
                this.SecurityTrailer = Marshal.ReadInt32(ptr, 12);
            }
        }
    }
}

