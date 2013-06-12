namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum TypeAttributes
    {
        Abstract = 0x80,
        AnsiClass = 0,
        AutoClass = 0x20000,
        AutoLayout = 0,
        BeforeFieldInit = 0x100000,
        Class = 0,
        ClassSemanticsMask = 0x20,
        CustomFormatClass = 0x30000,
        CustomFormatMask = 0xc00000,
        ExplicitLayout = 0x10,
        HasSecurity = 0x40000,
        Import = 0x1000,
        Interface = 0x20,
        LayoutMask = 0x18,
        NestedAssembly = 5,
        NestedFamANDAssem = 6,
        NestedFamily = 4,
        NestedFamORAssem = 7,
        NestedPrivate = 3,
        NestedPublic = 2,
        NotPublic = 0,
        Public = 1,
        ReservedMask = 0x40800,
        RTSpecialName = 0x800,
        Sealed = 0x100,
        SequentialLayout = 8,
        Serializable = 0x2000,
        SpecialName = 0x400,
        StringFormatMask = 0x30000,
        UnicodeClass = 0x10000,
        VisibilityMask = 7
    }
}

