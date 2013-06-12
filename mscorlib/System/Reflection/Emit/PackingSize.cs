namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum PackingSize
    {
        Size1 = 1,
        Size128 = 0x80,
        Size16 = 0x10,
        Size2 = 2,
        Size32 = 0x20,
        Size4 = 4,
        Size64 = 0x40,
        Size8 = 8,
        Unspecified = 0
    }
}

