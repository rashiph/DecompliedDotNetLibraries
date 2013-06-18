namespace System.ServiceModel.Channels
{
    using System;

    internal interface ILockingQueue
    {
        void DeleteMessage(long lookupId, TimeSpan timeout);
        void UnlockMessage(long lookupId, TimeSpan timeout);
    }
}

