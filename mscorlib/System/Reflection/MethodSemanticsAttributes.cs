namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum MethodSemanticsAttributes
    {
        AddOn = 8,
        Fire = 0x20,
        Getter = 2,
        Other = 4,
        RemoveOn = 0x10,
        Setter = 1
    }
}

