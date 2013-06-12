namespace System.Net
{
    using System;

    public enum FtpStatusCode
    {
        AccountNeeded = 0x214,
        ActionAbortedLocalProcessingError = 0x1c3,
        ActionAbortedUnknownPageType = 0x227,
        ActionNotTakenFilenameNotAllowed = 0x229,
        ActionNotTakenFileUnavailable = 550,
        ActionNotTakenFileUnavailableOrBusy = 450,
        ActionNotTakenInsufficientSpace = 0x1c4,
        ArgumentSyntaxError = 0x1f5,
        BadCommandSequence = 0x1f7,
        CantOpenData = 0x1a9,
        ClosingControl = 0xdd,
        ClosingData = 0xe2,
        CommandExtraneous = 0xca,
        CommandNotImplemented = 0x1f6,
        CommandOK = 200,
        CommandSyntaxError = 500,
        ConnectionClosed = 0x1aa,
        DataAlreadyOpen = 0x7d,
        DirectoryStatus = 0xd4,
        EnteringPassive = 0xe3,
        FileActionAborted = 0x228,
        FileActionOK = 250,
        FileCommandPending = 350,
        FileStatus = 0xd5,
        LoggedInProceed = 230,
        NeedLoginAccount = 0x14c,
        NotLoggedIn = 530,
        OpeningData = 150,
        PathnameCreated = 0x101,
        RestartMarker = 110,
        SendPasswordCommand = 0x14b,
        SendUserCommand = 220,
        ServerWantsSecureSession = 0xea,
        ServiceNotAvailable = 0x1a5,
        ServiceTemporarilyNotAvailable = 120,
        SystemType = 0xd7,
        Undefined = 0
    }
}

