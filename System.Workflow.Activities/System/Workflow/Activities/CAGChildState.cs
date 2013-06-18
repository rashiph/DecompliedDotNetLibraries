namespace System.Workflow.Activities
{
    using System;

    [Serializable]
    internal enum CAGChildState : byte
    {
        Excuting = 2,
        Idle = 0,
        Pending = 1
    }
}

