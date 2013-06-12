namespace System.Xml.Serialization
{
    using System;

    internal enum TypeFlags
    {
        Abstract = 1,
        AmbiguousDataType = 0x80,
        CanBeAttributeValue = 8,
        CanBeElementValue = 0x20,
        CanBeTextValue = 0x10,
        CollapseWhitespace = 0x8000,
        CtorInaccessible = 0x20000,
        GenericInterface = 0x80000,
        HasCustomFormatter = 0x40,
        HasDefaultConstructor = 0x800,
        HasIsEmpty = 0x400,
        IgnoreDefault = 0x200,
        None = 0,
        OptionalValue = 0x10000,
        Reference = 2,
        Special = 4,
        Unsupported = 0x100000,
        UsePrivateImplementation = 0x40000,
        UseReflection = 0x4000,
        XmlEncodingNotRequired = 0x1000
    }
}

