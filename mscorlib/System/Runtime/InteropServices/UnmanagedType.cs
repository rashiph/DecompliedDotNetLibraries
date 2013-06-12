namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum UnmanagedType
    {
        AnsiBStr = 0x23,
        AsAny = 40,
        Bool = 2,
        BStr = 0x13,
        ByValArray = 30,
        ByValTStr = 0x17,
        Currency = 15,
        CustomMarshaler = 0x2c,
        Error = 0x2d,
        FunctionPtr = 0x26,
        I1 = 3,
        I2 = 5,
        I4 = 7,
        I8 = 9,
        IDispatch = 0x1a,
        Interface = 0x1c,
        IUnknown = 0x19,
        LPArray = 0x2a,
        LPStr = 20,
        LPStruct = 0x2b,
        LPTStr = 0x16,
        LPWStr = 0x15,
        R4 = 11,
        R8 = 12,
        SafeArray = 0x1d,
        Struct = 0x1b,
        SysInt = 0x1f,
        SysUInt = 0x20,
        TBStr = 0x24,
        U1 = 4,
        U2 = 6,
        U4 = 8,
        U8 = 10,
        VariantBool = 0x25,
        VBByRefStr = 0x22
    }
}

