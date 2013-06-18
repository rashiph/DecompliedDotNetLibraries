namespace System.Activities.DurableInstancing
{
    using System;

    internal enum CommandResult
    {
        CleanupInProgress = 13,
        HostLockExpired = 11,
        HostLockNotFound = 12,
        InstanceAlreadyExists = 5,
        InstanceAlreadyLockedToOwner = 14,
        InstanceCompleted = 7,
        InstanceLockLost = 6,
        InstanceLockNotAcquired = 2,
        InstanceNotFound = 1,
        KeyAlreadyExists = 3,
        KeyDisassociated = 8,
        KeyNotFound = 4,
        StaleInstanceVersion = 10,
        Success = 0,
        Unknown = 0x63
    }
}

