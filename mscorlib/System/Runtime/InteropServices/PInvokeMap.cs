namespace System.Runtime.InteropServices
{
    using System;

    [Serializable]
    internal enum PInvokeMap
    {
        BestFitDisabled = 0x20,
        BestFitEnabled = 0x10,
        BestFitMask = 0x30,
        BestFitUseAsm = 0x30,
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
        NoMangle = 1,
        PinvokeOLE = 0x20,
        SupportsLastError = 0x40,
        ThrowOnUnmappableCharDisabled = 0x2000,
        ThrowOnUnmappableCharEnabled = 0x1000,
        ThrowOnUnmappableCharMask = 0x3000,
        ThrowOnUnmappableCharUseAsm = 0x3000
    }
}

