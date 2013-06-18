namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;

    [Serializable, Flags]
    public enum CompensatorOptions
    {
        AbortPhase = 4,
        AllPhases = 7,
        CommitPhase = 2,
        FailIfInDoubtsRemain = 0x10,
        PreparePhase = 1
    }
}

