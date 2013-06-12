namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum PInvokeAttributes
    {
        BestFitDisabled = 0x20,
        BestFitEnabled = 0x10,
        BestFitMask = 0x30,
        BestFitUseAssem = 0,
        CallConvCdecl = 0x200,
        CallConvFastcall = 0x500,
        CallConvMask = 0x700,
        CallConvStdcall = 0x300,
        CallConvThiscall = 0x400,
        CallConvWinapi = 0x100,
        CharSetAnsi = 2,
        CharSetAuto = 6,
        CharSetMask = 6,
        CharSetNotSpec = 0,
        CharSetUnicode = 4,
        MaxValue = 0xffff,
        NoMangle = 1,
        SupportsLastError = 0x40,
        ThrowOnUnmappableCharDisabled = 0x2000,
        ThrowOnUnmappableCharEnabled = 0x1000,
        ThrowOnUnmappableCharMask = 0x3000,
        ThrowOnUnmappableCharUseAssem = 0
    }
}

