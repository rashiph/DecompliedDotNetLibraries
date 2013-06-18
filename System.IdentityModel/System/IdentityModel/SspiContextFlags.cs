namespace System.IdentityModel
{
    using System;

    [Flags]
    internal enum SspiContextFlags
    {
        AcceptAnonymous = 0x100000,
        AcceptExtendedError = 0x8000,
        AcceptIdentify = 0x80000,
        AcceptStream = 0x10000,
        AllocateMemory = 0x100,
        ChannelBindingAllowMissingBindings = 0x10000000,
        ChannelBindingProxyBindings = 0x4000000,
        Confidentiality = 0x10,
        Delegate = 1,
        InitAnonymous = 0x40000,
        InitExtendedError = 0x4000,
        InitIdentify = 0x20000,
        InitManualCredValidation = 0x80000,
        InitStream = 0x8000,
        MutualAuth = 2,
        ReplayDetect = 4,
        SequenceDetect = 8,
        UseSessionKey = 0x20,
        Zero = 0
    }
}

