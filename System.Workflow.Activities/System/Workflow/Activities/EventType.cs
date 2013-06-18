namespace System.Workflow.Activities
{
    using System;

    [Serializable]
    internal enum EventType : byte
    {
        DataChange = 1,
        InterActivity = 6,
        LockAcquisition = 4,
        MessageArrival = 3,
        StatusChange = 2,
        Timer = 0
    }
}

