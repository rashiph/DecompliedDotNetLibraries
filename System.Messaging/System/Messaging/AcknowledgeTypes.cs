namespace System.Messaging
{
    using System;

    [Flags]
    public enum AcknowledgeTypes
    {
        FullReachQueue = 5,
        FullReceive = 14,
        NegativeReceive = 8,
        None = 0,
        NotAcknowledgeReachQueue = 4,
        NotAcknowledgeReceive = 12,
        PositiveArrival = 1,
        PositiveReceive = 2
    }
}

