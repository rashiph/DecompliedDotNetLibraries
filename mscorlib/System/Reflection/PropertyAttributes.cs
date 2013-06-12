namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum PropertyAttributes
    {
        HasDefault = 0x1000,
        None = 0,
        Reserved2 = 0x2000,
        Reserved3 = 0x4000,
        Reserved4 = 0x8000,
        ReservedMask = 0xf400,
        RTSpecialName = 0x400,
        SpecialName = 0x200
    }
}

