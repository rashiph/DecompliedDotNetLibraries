namespace System.Net.Mail
{
    using System;

    public enum SmtpStatusCode
    {
        BadCommandSequence = 0x1f7,
        CannotVerifyUserWillAttemptDelivery = 0xfc,
        ClientNotPermitted = 0x1c6,
        CommandNotImplemented = 0x1f6,
        CommandParameterNotImplemented = 0x1f8,
        CommandUnrecognized = 500,
        ExceededStorageAllocation = 0x228,
        GeneralFailure = -1,
        HelpMessage = 0xd6,
        InsufficientStorage = 0x1c4,
        LocalErrorInProcessing = 0x1c3,
        MailboxBusy = 450,
        MailboxNameNotAllowed = 0x229,
        MailboxUnavailable = 550,
        MustIssueStartTlsFirst = 530,
        Ok = 250,
        ServiceClosingTransmissionChannel = 0xdd,
        ServiceNotAvailable = 0x1a5,
        ServiceReady = 220,
        StartMailInput = 0x162,
        SyntaxError = 0x1f5,
        SystemStatus = 0xd3,
        TransactionFailed = 0x22a,
        UserNotLocalTryAlternatePath = 0x227,
        UserNotLocalWillForward = 0xfb
    }
}

