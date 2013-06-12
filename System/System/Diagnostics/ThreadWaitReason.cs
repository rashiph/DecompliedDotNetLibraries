namespace System.Diagnostics
{
    using System;

    public enum ThreadWaitReason
    {
        Executive,
        FreePage,
        PageIn,
        SystemAllocation,
        ExecutionDelay,
        Suspended,
        UserRequest,
        EventPairHigh,
        EventPairLow,
        LpcReceive,
        LpcReply,
        VirtualMemory,
        PageOut,
        Unknown
    }
}

