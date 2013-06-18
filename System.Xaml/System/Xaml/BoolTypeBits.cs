namespace System.Xaml
{
    using System;

    [Flags]
    internal enum BoolTypeBits
    {
        AllValid = -65536,
        Ambient = 0x8000,
        Constructible = 1,
        ConstructionRequiresArguments = 0x20,
        Default = 0x49,
        MarkupExtension = 4,
        NameScope = 0x10,
        Nullable = 8,
        Public = 0x40,
        TrimSurroundingWhitespace = 0x1000,
        Unknown = 0x100,
        UsableDuringInitialization = 0x4000,
        WhitespaceSignificantCollection = 0x2000,
        XmlData = 2
    }
}

