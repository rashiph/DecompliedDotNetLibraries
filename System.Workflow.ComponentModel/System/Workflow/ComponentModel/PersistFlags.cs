namespace System.Workflow.ComponentModel
{
    using System;

    [Serializable, Flags]
    internal enum PersistFlags : byte
    {
        ForcePersist = 2,
        NeedsCompensation = 1
    }
}

