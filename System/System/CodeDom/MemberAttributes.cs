namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum MemberAttributes
    {
        Abstract = 1,
        AccessMask = 0xf000,
        Assembly = 0x1000,
        Const = 5,
        Family = 0x3000,
        FamilyAndAssembly = 0x2000,
        FamilyOrAssembly = 0x4000,
        Final = 2,
        New = 0x10,
        Overloaded = 0x100,
        Override = 4,
        Private = 0x5000,
        Public = 0x6000,
        ScopeMask = 15,
        Static = 3,
        VTableMask = 240
    }
}

