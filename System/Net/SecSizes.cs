namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal class SecSizes
    {
        public readonly int MaxToken;
        public readonly int MaxSignature;
        public readonly int BlockSize;
        public readonly int SecurityTrailer;
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
                try
                {
                    this.MaxToken = (int) ((uint) Marshal.ReadInt32(ptr));
                    this.MaxSignature = (int) ((uint) Marshal.ReadInt32(ptr, 4));
                    this.BlockSize = (int) ((uint) Marshal.ReadInt32(ptr, 8));
                    this.SecurityTrailer = (int) ((uint) Marshal.ReadInt32(ptr, 12));
                }
                catch (OverflowException)
                {
                    throw;
                }
            }
        }
    }
}

