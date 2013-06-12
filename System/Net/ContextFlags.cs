namespace System.Net
{
    using System;

    [Flags]
    internal enum ContextFlags
    {
        AcceptExtendedError = 0x8000,
        AcceptIdentify = 0x80000,
        AcceptIntegrity = 0x20000,
        AcceptStream = 0x10000,
        AllocateMemory = 0x100,
        AllowMissingBindings = 0x10000000,
        Confidentiality = 0x10,
        Connection = 0x800,
        Delegate = 1,
        InitExtendedError = 0x4000,
        InitIdentify = 0x20000,
        InitIntegrity = 0x10000,
        InitManualCredValidation = 0x80000,
        InitStream = 0x8000,
        InitUseSuppliedCreds = 0x80,
        MutualAuth = 2,
        ProxyBindings = 0x4000000,
        ReplayDetect = 4,
        SequenceDetect = 8,
        UseSessionKey = 0x20,
        Zero = 0
    }
}

