namespace System.DirectoryServices.Protocols
{
    using System;

    public enum ResultCode
    {
        AdminLimitExceeded = 11,
        AffectsMultipleDsas = 0x47,
        AliasDereferencingProblem = 0x24,
        AliasProblem = 0x21,
        AttributeOrValueExists = 20,
        AuthMethodNotSupported = 7,
        Busy = 0x33,
        CompareFalse = 5,
        CompareTrue = 6,
        ConfidentialityRequired = 13,
        ConstraintViolation = 0x13,
        EntryAlreadyExists = 0x44,
        InappropriateAuthentication = 0x30,
        InappropriateMatching = 0x12,
        InsufficientAccessRights = 50,
        InvalidAttributeSyntax = 0x15,
        InvalidDNSyntax = 0x22,
        LoopDetect = 0x36,
        NamingViolation = 0x40,
        NoSuchAttribute = 0x10,
        NoSuchObject = 0x20,
        NotAllowedOnNonLeaf = 0x42,
        NotAllowedOnRdn = 0x43,
        ObjectClassModificationsProhibited = 0x45,
        ObjectClassViolation = 0x41,
        OffsetRangeError = 0x3d,
        OperationsError = 1,
        Other = 80,
        ProtocolError = 2,
        Referral = 10,
        ReferralV2 = 9,
        ResultsTooLarge = 70,
        SaslBindInProgress = 14,
        SizeLimitExceeded = 4,
        SortControlMissing = 60,
        StrongAuthRequired = 8,
        Success = 0,
        TimeLimitExceeded = 3,
        Unavailable = 0x34,
        UnavailableCriticalExtension = 12,
        UndefinedAttributeType = 0x11,
        UnwillingToPerform = 0x35,
        VirtualListViewError = 0x4c
    }
}

