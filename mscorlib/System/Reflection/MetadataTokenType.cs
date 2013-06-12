namespace System.Reflection
{
    using System;

    [Serializable]
    internal enum MetadataTokenType
    {
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        BaseType = 0x72000000,
        CustomAttribute = 0xc000000,
        Event = 0x14000000,
        ExportedType = 0x27000000,
        FieldDef = 0x4000000,
        File = 0x26000000,
        GenericPar = 0x2a000000,
        InterfaceImpl = 0x9000000,
        Invalid = 0x7fffffff,
        ManifestResource = 0x28000000,
        MemberRef = 0xa000000,
        MethodDef = 0x6000000,
        MethodSpec = 0x2b000000,
        Module = 0,
        ModuleRef = 0x1a000000,
        Name = 0x71000000,
        ParamDef = 0x8000000,
        Permission = 0xe000000,
        Property = 0x17000000,
        Signature = 0x11000000,
        String = 0x70000000,
        TypeDef = 0x2000000,
        TypeRef = 0x1000000,
        TypeSpec = 0x1b000000
    }
}

