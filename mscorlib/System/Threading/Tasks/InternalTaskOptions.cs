namespace System.Threading.Tasks
{
    using System;

    [Serializable, Flags]
    internal enum InternalTaskOptions
    {
        ChildReplica = 0x100,
        ContinuationTask = 0x200,
        InternalOptionsMask = 0xff00,
        None = 0,
        PromiseTask = 0x400,
        QueuedByRuntime = 0x2000,
        SelfReplicating = 0x800
    }
}

