namespace System.Xml.Serialization
{
    using System;

    internal enum XmlAttributeFlags
    {
        AnyAttribute = 0x200,
        AnyElements = 0x100,
        Array = 2,
        ArrayItems = 8,
        Attribute = 0x20,
        ChoiceIdentifier = 0x400,
        Elements = 0x10,
        Enum = 1,
        Root = 0x40,
        Text = 4,
        Type = 0x80,
        XmlnsDeclarations = 0x800
    }
}

