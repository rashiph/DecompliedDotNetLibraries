namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum AttributeTargets
    {
        All = 0x7fff,
        Assembly = 1,
        Class = 4,
        Constructor = 0x20,
        Delegate = 0x1000,
        Enum = 0x10,
        Event = 0x200,
        Field = 0x100,
        GenericParameter = 0x4000,
        Interface = 0x400,
        Method = 0x40,
        Module = 2,
        Parameter = 0x800,
        Property = 0x80,
        ReturnValue = 0x2000,
        Struct = 8
    }
}

