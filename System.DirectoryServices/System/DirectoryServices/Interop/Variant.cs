namespace System.DirectoryServices.Interop
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    internal struct Variant
    {
        [FieldOffset(8)]
        public short boolvalue;
        [FieldOffset(8)]
        public IntPtr ptr1;
        [FieldOffset(12)]
        public IntPtr ptr2;
        [FieldOffset(2)]
        public ushort reserved1;
        [FieldOffset(4)]
        public ushort reserved2;
        [FieldOffset(6)]
        public ushort reserved3;
        [FieldOffset(0)]
        public ushort varType;
    }
}

