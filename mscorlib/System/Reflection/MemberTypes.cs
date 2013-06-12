namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum MemberTypes
    {
        All = 0xbf,
        Constructor = 1,
        Custom = 0x40,
        Event = 2,
        Field = 4,
        Method = 8,
        NestedType = 0x80,
        Property = 0x10,
        TypeInfo = 0x20
    }
}

