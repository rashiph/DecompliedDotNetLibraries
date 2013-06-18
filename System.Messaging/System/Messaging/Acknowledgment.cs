namespace System.Messaging
{
    using System;

    public enum Acknowledgment
    {
        AccessDenied = 0x8004,
        BadDestinationQueue = 0x8000,
        BadEncryption = 0x8007,
        BadSignature = 0x8006,
        CouldNotEncrypt = 0x8008,
        HopCountExceeded = 0x8005,
        None = 0,
        NotTransactionalMessage = 0x800a,
        NotTransactionalQueue = 0x8009,
        Purged = 0x8001,
        QueueDeleted = 0xc000,
        QueueExceedMaximumSize = 0x8003,
        QueuePurged = 0xc001,
        ReachQueue = 2,
        ReachQueueTimeout = 0x8002,
        Receive = 0x4000,
        ReceiveTimeout = 0xc002
    }
}

