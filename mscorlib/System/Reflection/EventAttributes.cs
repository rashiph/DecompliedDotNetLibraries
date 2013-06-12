namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum EventAttributes
    {
        None = 0,
        ReservedMask = 0x400,
        RTSpecialName = 0x400,
        SpecialName = 0x200
    }
}

