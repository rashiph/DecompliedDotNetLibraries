namespace System.CodeDom.Compiler
{
    using System;

    [Serializable, Flags]
    public enum GeneratorSupport
    {
        ArraysOfArrays = 1,
        AssemblyAttributes = 0x1000,
        ChainedConstructorArguments = 0x8000,
        ComplexExpressions = 0x80000,
        DeclareDelegates = 0x200,
        DeclareEnums = 0x100,
        DeclareEvents = 0x800,
        DeclareIndexerProperties = 0x2000000,
        DeclareInterfaces = 0x400,
        DeclareValueTypes = 0x80,
        EntryPointMethod = 2,
        GenericTypeDeclaration = 0x1000000,
        GenericTypeReference = 0x800000,
        GotoStatements = 4,
        MultidimensionalArrays = 8,
        MultipleInterfaceMembers = 0x20000,
        NestedTypes = 0x10000,
        ParameterAttributes = 0x2000,
        PartialTypes = 0x400000,
        PublicStaticMembers = 0x40000,
        ReferenceParameters = 0x4000,
        Resources = 0x200000,
        ReturnTypeAttributes = 0x40,
        StaticConstructors = 0x10,
        TryCatchStatements = 0x20,
        Win32Resources = 0x100000
    }
}

