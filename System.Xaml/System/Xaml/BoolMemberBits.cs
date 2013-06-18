namespace System.Xaml
{
    using System;

    internal enum BoolMemberBits
    {
        AllValid = -65536,
        Ambient = 0x10,
        Default = 0x60,
        Directive = 0x60,
        Event = 4,
        ReadOnly = 1,
        ReadPublic = 0x20,
        Unknown = 8,
        WriteOnly = 2,
        WritePublic = 0x40
    }
}

