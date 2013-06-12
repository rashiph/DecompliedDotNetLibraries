namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    internal struct MetadataFieldOffset
    {
        public int FieldToken;
        public int Offset;
    }
}

