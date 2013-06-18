namespace System.DirectoryServices.Protocols
{
    using System;

    internal enum LdapError
    {
        AuthUnknown = 0x56,
        ClientLoop = 0x60,
        ConnectError = 0x5b,
        ControlNotFound = 0x5d,
        DecodingError = 0x54,
        EncodingError = 0x53,
        FilterError = 0x57,
        InvalidCredentials = 0x31,
        IsLeaf = 0x23,
        LocalError = 0x52,
        MoreResults = 0x5f,
        NoMemory = 90,
        NoResultsReturned = 0x5e,
        NotSupported = 0x5c,
        ParameterError = 0x59,
        ReferralLimitExceeded = 0x61,
        SendTimeOut = 0x70,
        ServerDown = 0x51,
        TimeOut = 0x55,
        UserCancelled = 0x58
    }
}

