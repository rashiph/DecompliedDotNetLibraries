namespace System.Diagnostics.SymbolStore
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum SymAddressKind
    {
        BitField = 9,
        ILOffset = 1,
        NativeOffset = 5,
        NativeRegister = 3,
        NativeRegisterRegister = 6,
        NativeRegisterRelative = 4,
        NativeRegisterStack = 7,
        NativeRVA = 2,
        NativeSectionOffset = 10,
        NativeStackRegister = 8
    }
}

