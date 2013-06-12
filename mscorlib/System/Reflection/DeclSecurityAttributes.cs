namespace System.Reflection
{
    using System;

    [Serializable, Flags]
    internal enum DeclSecurityAttributes
    {
        ActionMask = 0x1f,
        ActionNil = 0,
        Assert = 3,
        Demand = 2,
        Deny = 4,
        InheritanceCheck = 7,
        LinktimeCheck = 6,
        MaximumValue = 15,
        NonCasDemand = 13,
        NonCasInheritance = 15,
        NonCasLinkDemand = 14,
        PermitOnly = 5,
        PrejitDenied = 12,
        PrejitGrant = 11,
        Request = 1,
        RequestMinimum = 8,
        RequestOptional = 9,
        RequestRefuse = 10
    }
}

