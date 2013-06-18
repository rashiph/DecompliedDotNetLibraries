namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecurityBufferStruct
    {
        public int count;
        public BufferType type;
        public IntPtr token;
        public static readonly int Size;
        static SecurityBufferStruct()
        {
            Size = sizeof(SecurityBufferStruct);
        }
    }
}

