namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum ParameterAttributes
    {
        HasDefault = 0x1000,
        HasFieldMarshal = 0x2000,
        In = 1,
        Lcid = 4,
        None = 0,
        Optional = 0x10,
        Out = 2,
        Reserved3 = 0x4000,
        Reserved4 = 0x8000,
        ReservedMask = 0xf000,
        Retval = 8
    }
}

